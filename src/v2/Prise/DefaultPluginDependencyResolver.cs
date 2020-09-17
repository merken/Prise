using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Prise.V2
{
    public class DefaultPluginDependencyResolver : IPluginDependencyResolver
    {
        protected readonly IRuntimePlatformContext runtimePlatformContext;
        protected bool disposed = false;

        public DefaultPluginDependencyResolver(IRuntimePlatformContext runtimePlatformContext)
        {
            this.runtimePlatformContext = runtimePlatformContext;
        }

        public virtual Stream ResolvePluginDependencyToPath(string fullPathToPluginAssembly, PluginDependency dependency)
        {
            var localFile = Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), dependency.DependencyPath);
            if (File.Exists(localFile))
            {
                return File.OpenRead(localFile);
            }

            // TODO Add support for additional probing paths

            foreach (var candidate in runtimePlatformContext.GetPluginDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                var candidateLocation = Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), candidate);
                if (File.Exists(candidateLocation))
                {
                    return File.OpenRead(candidateLocation);
                }
            }
            return null;
        }

        public virtual string ResolvePlatformDependencyToPath(string fullPathToPluginAssembly, PlatformDependency dependency)
        {
            foreach (var candidate in runtimePlatformContext.GetPlatformDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                var candidateLocation = Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), candidate);
                if (File.Exists(candidateLocation))
                {
                    return candidateLocation;
                }
            }

            var local = Path.Combine(Path.GetDirectoryName(fullPathToPluginAssembly), dependency.DependencyPath);
            if (File.Exists(local))
            {
                return local;
            }

            // Todo add support for custom probing paths
            return null;
        }

        public virtual string ResolvePlatformDependencyPathToRuntime(PluginPlatformVersion pluginPlatformVersion, string platformDependencyPath)
        {
            var platformDependencyFileVersion = FileVersionInfo.GetVersionInfo(platformDependencyPath);
            var platformDependencyName = Path.GetFileName(platformDependencyPath);
            var runtimeInformation = this.runtimePlatformContext.GetRuntimeInfo();

            var runtimes = runtimeInformation.Runtimes;
            if (pluginPlatformVersion.IsSpecified)
            {
                // First filter on specific version
                runtimes = runtimes.Where(r => r.Version == pluginPlatformVersion.Version);
                // Then, filter on target runtime, this is not always provided
                if (pluginPlatformVersion.Runtime != RuntimeType.UnSpecified)
                    runtimes = runtimes.Where(r => r.RuntimeType == pluginPlatformVersion.Runtime);

                if (!runtimes.Any())
                    throw new AssemblyLoadException($"Requisted platform was not installed {pluginPlatformVersion.Runtime} {pluginPlatformVersion.Version}");
            }

            foreach (var runtime in runtimes.OrderByDescending(r => r.Version))
            {
                var candidateFilePath = Directory.GetFiles(runtime.Location).FirstOrDefault(f => String.Compare(Path.GetFileName(f), platformDependencyName) == 0);
                if (!String.IsNullOrEmpty(candidateFilePath))
                {
                    var candidateFileVersion = FileVersionInfo.GetVersionInfo(candidateFilePath);
                    if (String.Compare(platformDependencyFileVersion.FileVersion, candidateFileVersion.FileVersion) == 0)
                        return candidateFilePath;
                }
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Nothing to do here
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