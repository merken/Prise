using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Infra = Prise.Infrastructure;
using Prise.Infrastructure.NetCore.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prise.Infrastructure.NetCore
{
    internal class LocalDiskAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyName pluginInfrastructureAssemblyName;
        private string rootPath;
        private string pluginPath;
        private AssemblyDependencyResolver resolver;
        protected DependencyLoadPreference dependencyLoadPreference;

        private bool isConfigured;
        private ConcurrentDictionary<string, bool> dependencies;

        public LocalDiskAssemblyLoadContext()
            : base(true) // This should always be collectible, since we do not expect to have a long-living plugin
        {
            this.pluginInfrastructureAssemblyName = typeof(Infra.PluginAttribute).Assembly.GetName();
        }

        internal void Configure(string rootPath, string pluginPath, DependencyLoadPreference dependencyLoadPreference, ConcurrentDictionary<string, bool> dependencies)
        {
            if (this.isConfigured)
                return;
            this.rootPath = rootPath;
            this.pluginPath = pluginPath;
            this.resolver = new AssemblyDependencyResolver(Path.Combine(rootPath, pluginPath));
            this.dependencyLoadPreference = dependencyLoadPreference;
            this.dependencies = dependencies;

            this.isConfigured = true;
        }

        private Assembly LoadFromRemote(AssemblyName assemblyName)
        {
            if (File.Exists(Path.Combine(this.rootPath, Path.Combine(this.pluginPath, $"{assemblyName.Name}.dll"))))
            {
                var assembly = LoadAssemblyAndReferences(assemblyName);
                dependencies[assembly.FullName] = true;
                return assembly;
            }
            return null;
        }

        private Assembly LoadAssemblyAndReferences(AssemblyName assemblyName)
        {
            var assembly = LoadDependencyFromLocalDisk(assemblyName);
            if (assembly == null)
                return null;
            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                if (!referencedAssemblyName.FullName.Contains("netstandard"))
                    if (!dependencies.ContainsKey(referencedAssemblyName.FullName) || dependencies[referencedAssemblyName.FullName] == false)
                    {
                        // load all referenced assemblies from Remote
                        var referencedAssembly = LoadAssemblyAndReferences(referencedAssemblyName);
                        if (referencedAssembly == null)
                            throw new NotSupportedException($"Reference assembly {referencedAssemblyName.FullName} for {assemblyName.FullName} could not be loaded, did you publish the plugin?");
                        dependencies[referencedAssemblyName.FullName] = true;
                    }
            }
            return assembly;
        }

        private Assembly LoadFromDependencyContext(AssemblyName assemblyName)
        {
            string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                var assembly = LoadFromAssemblyPath(assemblyPath);
                if (assembly != null)
                    dependencies[assembly.FullName] = true;
                return assembly;
            }
            return null;
        }

        private Assembly LoadFromAppDomain(AssemblyName assemblyName)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                    dependencies[assembly.FullName] = true;
                return assembly;
            }
            catch (System.IO.FileNotFoundException) { }
            return null;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.FullName == this.pluginInfrastructureAssemblyName.FullName)
                return null;

            return AssemblyLoadStrategyFactory
                .GetAssemblyLoadStrategy(this.dependencyLoadPreference).LoadAssembly(
                    assemblyName,
                    LoadFromDependencyContext,
                    LoadFromRemote,
                    LoadFromAppDomain
                );
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }

        protected virtual Assembly LoadDependencyFromLocalDisk(AssemblyName assemblyName)
        {
            var name = $"{assemblyName.Name}.dll";
            var dependency = LoadFileFromLocalDisk(Path.Combine(this.rootPath, this.pluginPath), name);

            if (dependency == null) return null;

            return Assembly.Load(ToByteArray(dependency));
        }

        internal static Stream LoadFileFromLocalDisk(string loadPath, string pluginAssemblyName)
        {
            var probingPath = EnsureFileExists(loadPath, pluginAssemblyName);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(probingPath, FileMode.Open, FileAccess.Read))
            {
                memoryStream.SetLength(stream.Length);
                stream.Read(memoryStream.GetBuffer(), 0, (int)stream.Length);
            }
            return memoryStream;
        }

        internal static async Task<Stream> LoadFileFromLocalDiskAsync(string loadPath, string pluginAssemblyName)
        {
            var probingPath = EnsureFileExists(loadPath, pluginAssemblyName);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(probingPath, FileMode.Open, FileAccess.Read))
            {
                memoryStream.SetLength(stream.Length);
                await stream.ReadAsync(memoryStream.GetBuffer(), 0, (int)stream.Length);
            }
            return memoryStream;
        }

        private static string EnsureFileExists(string loadPath, string pluginAssemblyName)
        {
            var probingPath = Path.GetFullPath(Path.Combine(loadPath, pluginAssemblyName)).Replace("\\", "/");
            if (!File.Exists(probingPath))
                throw new FileNotFoundException($"Plugin assembly does not exist in path : {probingPath}");
            return probingPath;
        }

        internal static byte[] ToByteArray(Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }
    }

    public class LocalDiskAssemblyLoader<T> : DisposableAssemblyUnLoader, IPluginAssemblyLoader<T>
    {
        private readonly IRootPathProvider rootPathProvider;
        private readonly ILocalAssemblyLoaderOptions options;
        private readonly LocalDiskAssemblyLoadContext context;

        public LocalDiskAssemblyLoader(IRootPathProvider rootPathProvider, ILocalAssemblyLoaderOptions options)
        {
            this.rootPathProvider = rootPathProvider;
            this.options = options;
            this.context = new LocalDiskAssemblyLoadContext();
            this.loadContext = this.context;
        }

        public Assembly Load(string pluginAssemblyName)
        {
            // var rootPluginPath = Path.Join(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            // var pluginStream = LocalDiskAssemblyLoadContext.LoadFileFromLocalDisk(rootPluginPath, pluginAssemblyName);
            // this.context.Configure(this.rootPathProvider.GetRootPath(), this.options.PluginPath, this.options.DependencyLoadPreference);
            // return this.context.LoadFromStream(pluginStream);

            var rootPluginPath = Path.Join(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            var pluginStream = LocalDiskAssemblyLoadContext.LoadFileFromLocalDisk(rootPluginPath, pluginAssemblyName);
            // var pluginStream2 = LocalDiskAssemblyLoadContext.LoadFileFromLocalDisk(rootPluginPath, pluginAssemblyName);
            var dependencies = GetDependencies(Path.Join(this.rootPathProvider.GetRootPath(), this.options.PluginPath), pluginAssemblyName);
            this.context.Configure(this.rootPathProvider.GetRootPath(), this.options.PluginPath, this.options.DependencyLoadPreference, dependencies);
            var assembly = this.context.LoadFromStream(pluginStream);
            foreach (var dependency in dependencies.Keys)
            {
                var dependencyAssembly = LoadAssembly(Path.Join(rootPluginPath, $"{dependency}.dll"));
                if (dependencyAssembly != null)
                    this.context.LoadFromStream(dependencyAssembly);
            }
            return assembly;
        }

        private Stream LoadAssembly(string path)
        {
            if (!File.Exists(path))
                return null;
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                memoryStream.SetLength(stream.Length);
                stream.Read(memoryStream.GetBuffer(), 0, (int)stream.Length);
            }
            return memoryStream;
        }

        public async Task<Assembly> LoadAsync(string pluginAssemblyName)
        {
            var rootPluginPath = Path.Join(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            var pluginStream = await LocalDiskAssemblyLoadContext.LoadFileFromLocalDiskAsync(rootPluginPath, pluginAssemblyName);
            this.context.Configure(this.rootPathProvider.GetRootPath(), this.options.PluginPath, this.options.DependencyLoadPreference, GetDependencies(pluginStream));
            return this.context.LoadFromStream(pluginStream);
        }

        private ConcurrentDictionary<string, bool> GetDependencies(string assemblyLocation, string assemblyName)
        {
            var dependencies = new ConcurrentDictionary<string, bool>();
            using (var stream = new StreamReader(Path.Join(assemblyLocation, $"{Path.GetFileNameWithoutExtension(assemblyName)}.deps.json")))
            {
                var json = stream.ReadToEnd();
                var file = JsonSerializer.Deserialize<DependencyFile>(json);
                var projectReferences = file.Libraries.Where(l => l.Value.Type == "project" && !l.Key.StartsWith("Contract"));
                var otherReferencesExceptRuntime = file.Libraries
                    .Where(l =>
                        !l.Key.StartsWith("runtime") &&
                        !l.Key.StartsWith("Microsoft.CSharp") &&
                        !l.Key.StartsWith("NETStandard") &&
                        !l.Key.StartsWith("Microsoft.NETCore") &&
                        !l.Key.StartsWith("Contract") &&
                        !l.Key.StartsWith("Microsoft.Win32") &&
                        (l.Value.Type == "package" && l.Value.Serviceable));

                foreach (var reference in projectReferences.Union(otherReferencesExceptRuntime))
                    dependencies[reference.Key.Split('/')[0]] = false;
            }
            return dependencies;
        }

        private ConcurrentDictionary<string, bool> GetDependencies(Stream stream)
        {
            // var depsFile = 
            var assembly = Assembly.Load(LocalDiskAssemblyLoadContext.ToByteArray(stream));
            var dependencies = new ConcurrentDictionary<string, bool>();
            foreach (var dependency in assembly.GetReferencedAssemblies())
            {
                if (!dependency.FullName.Contains("netstandard"))
                    dependencies[dependency.FullName] = false;
            }
            return dependencies;
        }
    }

    public class DependencyFile
    {
        [JsonPropertyName("libraries")]
        public Dictionary<String, Library> Libraries { get; set; }
    }

    public class Library
    {
        [JsonPropertyName("type")]
        public String Type { get; set; }

        [JsonPropertyName("serviceable")]
        public bool Serviceable { get; set; }
    }
}