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
        private readonly string rootPath;
        private readonly string pluginLocation;
        private readonly AssemblyName pluginInfrastructureAssemblyName;
        private AssemblyDependencyResolver resolver;

        public LocalDiskAssemblyLoadContext(string rootPath, string pluginLocation)
        {
            this.rootPath = rootPath;
            this.pluginLocation = pluginLocation;
            this.pluginInfrastructureAssemblyName = typeof(Infra.PluginAttribute).Assembly.GetName();
            this.resolver = new AssemblyDependencyResolver(rootPath);
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
            if (!File.Exists(Path.Combine(this.rootPath, Path.Combine(this.pluginLocation, $"{assemblyName.Name}.dll"))))
                return null;

            var name = $"{assemblyName.Name}.dll";
            var dependency = LoadFileFromLocalDisk(Path.Combine(this.rootPath, this.pluginLocation), name).Result;

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
        public LocalDiskAssemblyLoader(IRootPathProvider rootPathProvider, ILocalAssemblyLoaderOptions options)
        {
            this.rootPathProvider = rootPathProvider;
            this.options = options;
        }

        public async Task<Assembly> Load(string pluginAssemblyName)
        {
            var pluginStream = await LocalDiskAssemblyLoadContext.LoadFileFromLocalDisk(this.options.PluginPath, pluginAssemblyName);
            var loader = new LocalDiskAssemblyLoadContext(this.rootPathProvider.GetRootPath(), this.options.PluginPath);
            return loader.LoadFromStream(pluginStream);
        }
    }
}