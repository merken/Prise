using System;
using System.Reflection;

namespace Prise.Infrastructure.NetCore
{
    internal interface IAssemblyLoadStrategy
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
            Func<AssemblyName, ValueOrProceed<Assembly>> loadFromDependencyContext,
            Func<AssemblyName, ValueOrProceed<Assembly>> loadFromRemote,
            Func<AssemblyName, ValueOrProceed<Assembly>> loadFromAppDomain);

        /// <summary>
        /// Loads a native assembly
        /// </summary>
        /// <param name="unmanagedDllName"></param>
        /// <param name="loadFromDependencyContext"></param>
        /// <param name="loadFromRemote"></param>
        /// <param name="loadFromAppDomain"></param>
        /// <returns>The path to a native assembly</returns>
        NativeAssembly LoadUnmanagedDll(string unmanagedDllName,
           Func<string, ValueOrProceed<string>> loadFromDependencyContext,
           Func<string, ValueOrProceed<string>> loadFromRemote,
           Func<string, ValueOrProceed<IntPtr>> loadFromAppDomain);
    }
}
