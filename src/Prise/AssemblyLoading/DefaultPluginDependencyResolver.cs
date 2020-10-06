using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Prise.Core;
using Prise.Utils;

namespace Prise.AssemblyLoading
{
    public class DefaultPluginDependencyResolver : IPluginDependencyResolver
    {
        protected IRuntimePlatformContext runtimePlatformContext;

        public DefaultPluginDependencyResolver(Func<IRuntimePlatformContext> runtimePlatformContextFactory)
        {
            this.runtimePlatformContext = runtimePlatformContextFactory.ThrowIfNull(nameof(runtimePlatformContextFactory))();
        }

        public virtual Stream ResolvePluginDependencyToPath(string initialPluginLoadDirectory, PluginDependency dependency, IEnumerable<string> additionalProbingPaths)
        {
            var localFile = Path.Combine(initialPluginLoadDirectory, dependency.DependencyPath);
            if (File.Exists(localFile))
            {
                return File.OpenRead(localFile);
            }

            foreach (var probingPath in additionalProbingPaths)
            {
                var candidate = Path.Combine(probingPath, dependency.ProbingPath);
                if (File.Exists(candidate))
                {
                    return File.OpenRead(candidate);
                }
            }

            foreach (var candidate in runtimePlatformContext.GetPluginDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                var candidateLocation = Path.Combine(initialPluginLoadDirectory, candidate);
                if (File.Exists(candidateLocation))
                {
                    return File.OpenRead(candidateLocation);
                }
            }
            return null;
        }

        public virtual string ResolvePlatformDependencyToPath(string initialPluginLoadDirectory, PlatformDependency dependency, IEnumerable<string> additionalProbingPaths)
        {
            foreach (var candidate in runtimePlatformContext.GetPlatformDependencyNames(dependency.DependencyNameWithoutExtension))
            {
                var candidateLocation = Path.Combine(initialPluginLoadDirectory, candidate);
                if (File.Exists(candidateLocation))
                {
                    return candidateLocation;
                }
            }

            var local = Path.Combine(initialPluginLoadDirectory, dependency.DependencyPath);
            if (File.Exists(local))
            {
                return local;
            }

            foreach (var searchPath in additionalProbingPaths)
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
                    throw new AssemblyLoadingException($"Requisted platform was not installed {pluginPlatformVersion.Runtime} {pluginPlatformVersion.Version}");
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

        protected bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.runtimePlatformContext = null;
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