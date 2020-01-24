using Prise.Infrastructure;
using System;
using System.IO;

namespace Prise
{
    public class UserProfileTempPathProvider<T> : ITempPathProvider<T>
    {
        private const string PluginTempPath = "plugins";
        private string tempPath = String.Empty;
        private bool disposed = false;

        public string ProvideTempPath(string assemblyName) => Path.Join(EnsureTempPath(assemblyName), assemblyName);

        private string EnsureTempPath(string assemblyName)
        {
            if (!String.IsNullOrEmpty(tempPath))
                return tempPath;

            var systemTempPath = System.IO.Path.GetTempPath();
            var randomTempPath = $"{Path.Join(systemTempPath, Path.Join(PluginTempPath, assemblyName))}";
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
