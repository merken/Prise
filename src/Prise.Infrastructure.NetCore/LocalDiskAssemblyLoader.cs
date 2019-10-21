using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Prise.Infrastructure.NetCore.Contracts;

namespace Prise.Infrastructure.NetCore
{
    internal class LocalDiskAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyName pluginInfrastructureAssemblyName;
        private string rootPath;
        private string pluginPath;
        private bool isConfigured;

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

            var deps = DependencyContext.Default;
            var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();
            if (res.Count > 0)
            {
                return Assembly.Load(new AssemblyName(res.First().Name));
            }
            else
            {
                if (File.Exists(Path.Combine(this.rootPath, Path.Combine(this.pluginPath, $"{assemblyName.Name}.dll"))))
                {
                    return LoadDependencyFromLocalDisk(assemblyName);
                }
            }
            return Assembly.Load(assemblyName);
        }

        protected virtual Assembly LoadDependencyFromLocalDisk(AssemblyName assemblyName)
        {
            var name = $"{assemblyName.Name}.dll";
            var dependency = LoadFileFromLocalDisk(Path.Combine(this.rootPath, this.pluginPath), name).Result;

            if (dependency == null) return null;

            return Assembly.Load(ToByteArray(dependency));
        }

        internal static async Task<Stream> LoadFileFromLocalDisk(string loadPath, string pluginAssemblyName)
        {
            var probingPath = Path.GetFullPath(Path.Combine(loadPath, pluginAssemblyName)).Replace("\\", "/");
            if (!File.Exists(probingPath))
                throw new FileNotFoundException($"Plugin assembly does not exist in path : {probingPath}");

            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(probingPath, FileMode.Open, FileAccess.Read))
                await stream.CopyToAsync(memoryStream);

            memoryStream.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public static byte[] ToByteArray(Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }
    }

    public class LocalDiskAssemblyLoader<T> : IPluginAssemblyLoader<T>
    {
        protected readonly IRootPathProvider rootPathProvider;
        protected readonly ILocalAssemblyLoaderOptions options;
        private readonly LocalDiskAssemblyLoadContext context;
        protected bool disposed = false;

        public LocalDiskAssemblyLoader(IRootPathProvider rootPathProvider, ILocalAssemblyLoaderOptions options)
        {
            this.rootPathProvider = rootPathProvider;
            this.options = options;
            this.context = new LocalDiskAssemblyLoadContext();
        }

        public async Task<Assembly> Load(string pluginAssemblyName)
        {
            var rootPluginPath = Path.Join(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            var pluginStream = await LocalDiskAssemblyLoadContext.LoadFileFromLocalDisk(rootPluginPath, pluginAssemblyName);
            this.context.Configure(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            return this.context.LoadFromStream(pluginStream);
        }

        public async virtual Task Unload()
        {
            GC.Collect(); // collects all unused memory
            GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            GC.Collect();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                GC.Collect(); // collects all unused memory
                GC.WaitForPendingFinalizers(); // wait until GC has finished its work
                GC.Collect();
            }
            this.disposed = true;
        }
    }
}