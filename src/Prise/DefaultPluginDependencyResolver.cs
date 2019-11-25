using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Prise.Infrastructure;

namespace Prise
{
    public class DefaultPluginDependencyResolver<T> : IPluginDependencyResolver<T>
    {
        protected readonly IRuntimePlatformContext runtimePlatformContext;
        protected bool disposed = false;

        public DefaultPluginDependencyResolver(IRuntimePlatformContext runtimePlatformContext)
        {
            this.runtimePlatformContext = runtimePlatformContext;
        }

        public virtual Stream ResolvePluginDependencyToPath(string dependencyPath, IEnumerable<string> probingPaths, PluginDependency dependency)
        {
            var localFile = Path.Combine(dependencyPath, dependency.DependencyPath);
            if (File.Exists(localFile))
            {
                return File.OpenRead(localFile);
            }

            foreach (var probingPath in probingPaths)
            {
                var candidate = Path.Combine(probingPath, dependency.ProbingPath);
                if (File.Exists(candidate))
                {
                    return File.OpenRead(candidate);
                }
            }

            foreach (var candidate in runtimePlatformContext.GetPluginDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                var candidateLocation = Path.Combine(dependencyPath, candidate);
                if (File.Exists(candidateLocation))
                {
                    return File.OpenRead(candidateLocation);
                }
            }
            return null;
        }

        public virtual string ResolvePlatformDependencyToPath(string dependencyPath, IEnumerable<string> probingPaths, PlatformDependency dependency)
        {
            foreach (var candidate in runtimePlatformContext.GetPlatformDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                var candidateLocation = Path.Combine(dependencyPath, candidate);
                if (File.Exists(candidateLocation))
                {
                    return candidateLocation;
                }
            }

            var local = Path.Combine(dependencyPath, dependency.DependencyPath);
            if (File.Exists(local))
            {
                return local;
            }

            foreach (var searchPath in probingPaths)
            {
                var candidate = Path.Combine(searchPath, dependency.ProbingPath);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        public virtual string ResolvePlatformDependencyPathToRuntime(PluginPlatformVersion pluginPlatformVersion, string platformDependencyPath)
        {
            var platformDependencyFileVersion = FileVersionInfo.GetVersionInfo(platformDependencyPath);
            var platformDependencyName = Path.GetFileName(platformDependencyPath);
            var runtimeInformation = this.runtimePlatformContext.GetRuntimeInformation();

            var runtimes = runtimeInformation.Runtimes;
            if (pluginPlatformVersion.IsSpecified)
            {
                // First filter on specific version
                runtimes = runtimes.Where(r => r.Version == pluginPlatformVersion.Version);
                // Then, filter on target runtime, this is not always provided
                if (pluginPlatformVersion.Runtime != RuntimeType.UnSpecified)
                    runtimes = runtimes.Where(r => r.RuntimeType == pluginPlatformVersion.Runtime);

                if (!runtimes.Any())
                    throw new PrisePluginException($"Requisted platform was not installed {pluginPlatformVersion.Runtime} {pluginPlatformVersion.Version}");
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

            return String.Empty;
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
