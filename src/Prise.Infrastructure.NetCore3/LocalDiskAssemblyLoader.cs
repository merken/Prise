using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Infra = Prise.Infrastructure;
using Prise.Infrastructure.NetCore.Contracts;

namespace Prise.Infrastructure.NetCore3
{
    internal class LocalDiskAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyName pluginInfrastructureAssemblyName;
        private string rootPath;
        private string pluginPath;
        private AssemblyDependencyResolver resolver;
        private bool isConfigured;

        public LocalDiskAssemblyLoadContext()
            : base(true) // This should always be collectible, since we do not expect to have a long-living plugin
        {
            this.pluginInfrastructureAssemblyName = typeof(Infra.PluginAttribute).Assembly.GetName();
        }

        internal void Configure(string rootPath, string pluginPath)
        {
            if (this.isConfigured)
                throw new NotSupportedException($"This LocalDiskAssemblyLoadContext is already configured for {this.rootPath} {this.pluginPath}. Could not configure for {rootPath} {pluginPath}");

            this.rootPath = rootPath;
            this.pluginPath = pluginPath;
            this.resolver = new AssemblyDependencyResolver(rootPath);

            this.isConfigured = true;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.FullName == this.pluginInfrastructureAssemblyName.FullName)
                return null;

            string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return LoadDependencyFromLocalDisk(assemblyName);
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
            if (!File.Exists(Path.Combine(this.rootPath, Path.Combine(this.pluginPath, $"{assemblyName.Name}.dll"))))
                return null;

            var name = $"{assemblyName.Name}.dll";
            var dependency = LoadFileFromLocalDisk(Path.Combine(this.rootPath, this.pluginPath), name).Result;

            if (dependency == null) return null;

            return Assembly.Load(ToByteArray(dependency));
        }

        internal static async Task<Stream> LoadFileFromLocalDisk(string loadPath, string pluginAssemblyName)
        {
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(Path.Combine(loadPath, pluginAssemblyName), FileMode.Open, FileAccess.Read))
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
        private readonly IRootPathProvider rootPathProvider;
        private readonly ILocalAssemblyLoaderOptions options;
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
            var pluginStream = await LocalDiskAssemblyLoadContext.LoadFileFromLocalDisk(this.options.PluginPath, pluginAssemblyName);
            this.context.Configure(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            return this.context.LoadFromStream(pluginStream);
        }

        public Task Unload()
        {
            this.context.Unload();
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.context.Unload();
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}