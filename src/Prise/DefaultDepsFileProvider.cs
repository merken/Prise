using Prise.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Prise
{
    public class DefaultDepsFileProvider<T> : IDepsFileProvider<T>
    {
        private Stream stream;
        private bool disposed = false;

        public async Task<Stream> ProvideDepsFile(IPluginLoadContext pluginLoadContext)
        {
            var pluginPath = pluginLoadContext.PluginAssemblyPath;
            var depsFileLocation = Path.GetFullPath(Path.Join(pluginPath, $"{Path.GetFileNameWithoutExtension(pluginLoadContext.PluginAssemblyName)}.deps.json"));
            this.stream = new MemoryStream();

            using (var fileStream = File.OpenRead(depsFileLocation))
                await fileStream.CopyToAsync(this.stream);

            stream.Seek(0, SeekOrigin.Begin);
            return this.stream;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.stream?.Dispose();
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
