using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Infra = Prise.Infrastructure;
using Prise.Infrastructure.NetCore.Contracts;

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

        public LocalDiskAssemblyLoadContext()
            : base(true) // This should always be collectible, since we do not expect to have a long-living plugin
        {
            this.pluginInfrastructureAssemblyName = typeof(Infra.PluginAttribute).Assembly.GetName();
        }

        internal void Configure(string rootPath, string pluginPath, DependencyLoadPreference dependencyLoadPreference)
        {
            if (this.isConfigured)
                return;
            this.rootPath = rootPath;
            this.pluginPath = pluginPath;
            this.resolver = new AssemblyDependencyResolver(rootPath);
            this.dependencyLoadPreference = dependencyLoadPreference;

            this.isConfigured = true;
        }

        private Assembly LoadFromRemote(AssemblyName assemblyName)
        {
            if (File.Exists(Path.Combine(this.rootPath, Path.Combine(this.pluginPath, $"{assemblyName.Name}.dll"))))
            {
                return LoadDependencyFromLocalDisk(assemblyName);
            }
            return null;
        }

        private Assembly LoadFromDependencyContext(AssemblyName assemblyName)
        {
            string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }

        private Assembly LoadFromAppDomain(AssemblyName assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
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

        private static byte[] ToByteArray(Stream stream)
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
            var rootPluginPath = Path.Join(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            var pluginStream = LocalDiskAssemblyLoadContext.LoadFileFromLocalDisk(rootPluginPath, pluginAssemblyName);
            this.context.Configure(this.rootPathProvider.GetRootPath(), this.options.PluginPath, this.options.DependencyLoadPreference);
            return this.context.LoadFromStream(pluginStream);
        }

        public async Task<Assembly> LoadAsync(string pluginAssemblyName)
        {
            var rootPluginPath = Path.Join(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            var pluginStream = await LocalDiskAssemblyLoadContext.LoadFileFromLocalDiskAsync(rootPluginPath, pluginAssemblyName);
            this.context.Configure(this.rootPathProvider.GetRootPath(), this.options.PluginPath, this.options.DependencyLoadPreference);
            return this.context.LoadFromStream(pluginStream);
        }
    }
}