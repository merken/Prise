using Prise.Infrastructure;
using System;
using System.Reflection;

namespace Prise
{
    public class AssemblyFromStrategy
    {
        public Assembly Assembly { get; set; }
        public bool CanBeReleased { get; set; }

        public static AssemblyFromStrategy Releasable(Assembly assembly) => new AssemblyFromStrategy() { Assembly = assembly, CanBeReleased = true };
        public static AssemblyFromStrategy NotReleasable(Assembly assembly) => new AssemblyFromStrategy() { Assembly = assembly, CanBeReleased = false };
    }

    public interface IAssemblyLoadStrategy
    {
        /// <summary>
        /// Loads a dependency assembly for the current plugin
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="loadFromDependencyContext"></param>
        /// <param name="loadFromRemote"></param>
        /// <param name="loadFromAppDomain"></param>
        /// <returns>A loaded assembly</returns>
        AssemblyFromStrategy LoadAssembly(AssemblyName assemblyName,
            Func<IPluginLoadContext, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromDependencyContext,
            Func<IPluginLoadContext, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromRemote,
            Func<IPluginLoadContext, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromAppDomain);

        /// <summary>
        /// Loads a native assembly
        /// </summary>
        /// <param name="unmanagedDllName"></param>
        /// <param name="loadFromDependencyContext"></param>
        /// <param name="loadFromRemote"></param>
        /// <param name="loadFromAppDomain"></param>
        /// <returns>The path to a native assembly</returns>
        NativeAssembly LoadUnmanagedDll(string unmanagedDllName,
           Func<IPluginLoadContext, string, ValueOrProceed<string>> loadFromDependencyContext,
           Func<IPluginLoadContext, string, ValueOrProceed<string>> loadFromRemote,
           Func<IPluginLoadContext, string, ValueOrProceed<IntPtr>> loadFromAppDomain);
    }
}
