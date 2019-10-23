using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Prise.Infrastructure.NetCore.Contracts;

namespace Prise.Infrastructure.NetCore
{
    internal class LocalDiskAssemblyLoadContext : AssemblyLoadContext
    {
        protected readonly AssemblyName pluginInfrastructureAssemblyName;
        protected string rootPath;
        protected string pluginPath;
        protected bool isConfigured;

        public LocalDiskAssemblyLoadContext()
        {
            this.pluginInfrastructureAssemblyName = typeof(PluginAttribute).Assembly.GetName();
        }

        internal void Configure(string rootPath, string pluginPath)
        {
            if (this.isConfigured)
                throw new NotSupportedException($"This LocalDiskAssemblyLoadContext is already configured for {this.rootPath} {this.pluginPath}. Could not configure for {rootPath} {pluginPath}");

            this.rootPath = rootPath;
            this.pluginPath = pluginPath;

            this.isConfigured = true;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.FullName == this.pluginInfrastructureAssemblyName.FullName)
                return null;

            var defaultDependencies = DependencyContext.Default;
            var candidateAssembly = defaultDependencies.CompileLibraries.FirstOrDefault(d => String.Compare(d.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase) == 0);
            if (candidateAssembly != null)
            {
                return Assembly.Load(new AssemblyName(candidateAssembly.Name));
            }

            if (!assemblyName.Name.ToLower().Contains("avalonia"))
            {

                // Check if the dependency exists in the directory of the plugin
                if (File.Exists(Path.Combine(this.rootPath, Path.Combine(this.pluginPath, $"{assemblyName.Name}.dll"))))
                {
                    return LoadDependencyFromLocalDisk(assemblyName);
                }
            }

            // Default, just load the assembly as if it were in this AppDomain
            return Assembly.Load(assemblyName);
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
        protected readonly IRootPathProvider rootPathProvider;
        protected readonly ILocalAssemblyLoaderOptions options;
        private readonly LocalDiskAssemblyLoadContext context;

        public LocalDiskAssemblyLoader(IRootPathProvider rootPathProvider, ILocalAssemblyLoaderOptions options)
        {
            this.rootPathProvider = rootPathProvider;
            this.options = options;
            this.context = new LocalDiskAssemblyLoadContext();
        }

        public Assembly Load(string pluginAssemblyName)
        {
            var rootPluginPath = Path.Join(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            var pluginStream = LocalDiskAssemblyLoadContext.LoadFileFromLocalDisk(rootPluginPath, pluginAssemblyName);
            this.context.Configure(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            return this.context.LoadFromStream(pluginStream);
        }

        public async Task<Assembly> LoadAsync(string pluginAssemblyName)
        {
            var rootPluginPath = Path.Join(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            var pluginStream = await LocalDiskAssemblyLoadContext.LoadFileFromLocalDiskAsync(rootPluginPath, pluginAssemblyName);
            this.context.Configure(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            return this.context.LoadFromStream(pluginStream);
        }
    }
}