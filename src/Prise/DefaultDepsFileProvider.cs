using Prise.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Prise
{
    public class DefaultDepsFileProvider : IDepsFileProvider
    {
        private readonly IPluginPathProvider pluginPathProvider;
        private Stream stream;
        private bool disposed = false;

        public DefaultDepsFileProvider(IPluginPathProvider pluginPathProvider)
        {
            this.pluginPathProvider = pluginPathProvider;
        }

        public Task<Stream> ProvideDepsFile(string pluginAssemblyName)
        {
            var pluginPath = this.pluginPathProvider.GetPluginPath();
            var depsFileLocation = Path.GetFullPath(Path.Join(pluginPath, $"{Path.GetFileNameWithoutExtension(pluginAssemblyName)}.deps.json"));
            this.stream = new MemoryStream();

            using (var fileStream = File.OpenRead(depsFileLocation))
                fileStream.CopyTo(this.stream);

            stream.Seek(0, SeekOrigin.Begin);
            return Task<Stream>.FromResult(this.stream);
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
