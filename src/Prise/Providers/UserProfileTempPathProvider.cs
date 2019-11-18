using Prise.Infrastructure;
using System;
using System.IO;

namespace Prise
{
    public class UserProfileTempPathProvider : ITempPathProvider
    {
        private static string PluginTempPath = "plugins";
        private readonly string pluginAssemblyName;
        private string tempPath = String.Empty;
        private bool disposed = false;

        public UserProfileTempPathProvider(IPluginAssemblyNameProvider pluginAssemblyNameProvider)
        {
            this.pluginAssemblyName = Path.GetFileNameWithoutExtension(pluginAssemblyNameProvider.GetAssemblyName());
        }

        public string ProvideTempPath(string assemblyName) => Path.Join(EnsureTempPath(), assemblyName);

        private string EnsureTempPath()
        {
            if (!String.IsNullOrEmpty(tempPath))
                return tempPath;

            var systemTempPath = System.IO.Path.GetTempPath();
            var randomTempPath = $"{Path.Join(systemTempPath, Path.Join(PluginTempPath, this.pluginAssemblyName))}";
            if (!Directory.Exists(randomTempPath))
                Directory.CreateDirectory(randomTempPath);

            this.tempPath = randomTempPath;

            return this.tempPath;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.tempPath = null;
                // TODO check if we can do this
                //if (Directory.Exists(tempPath))
                //    Directory.Delete(tempPath, true);
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
