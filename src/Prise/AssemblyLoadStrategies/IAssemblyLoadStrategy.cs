using Prise.Infrastructure;
using System;
using System.Reflection;

namespace Prise
{
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
        Assembly LoadAssembly(AssemblyName assemblyName,
            Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> loadFromDependencyContext,
            Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> loadFromRemote,
            Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> loadFromAppDomain);

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
