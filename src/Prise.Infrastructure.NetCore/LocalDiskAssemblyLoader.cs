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
        private readonly string rootPath;
        private readonly string pluginPath;
        private readonly AssemblyName pluginInfrastructureAssemblyName;

        public LocalDiskAssemblyLoadContext(string rootPath, string pluginPath)
        {
            this.rootPath = rootPath;
            this.pluginPath = pluginPath;
            this.pluginInfrastructureAssemblyName = typeof(Prise.Infrastructure.PluginAttribute).Assembly.GetName();
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
            if (!File.Exists(Path.Combine(loadPath, pluginAssemblyName)))
                throw new FileNotFoundException($"Plugin assembly does not exist in path : {Path.Combine(loadPath, pluginAssemblyName)}");

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
        protected readonly IRootPathProvider rootPathProvider;
        protected readonly ILocalAssemblyLoaderOptions options;

        public LocalDiskAssemblyLoader(IRootPathProvider rootPathProvider, ILocalAssemblyLoaderOptions options)
        {
            this.rootPathProvider = rootPathProvider;
            this.options = options;
        }

        public async virtual Task<Assembly> Load(string pluginAssemblyName)
        {
            var rootPath = this.rootPathProvider.GetRootPath();
            var pluginAbsolutePath = Path.Combine(rootPath, this.options.PluginPath);
            var pluginStream = await LocalDiskAssemblyLoadContext.LoadFileFromLocalDisk(pluginAbsolutePath, pluginAssemblyName);
            var loader = new LocalDiskAssemblyLoadContext(rootPath, this.options.PluginPath);
            return loader.LoadFromStream(pluginStream);
        }
    }
}