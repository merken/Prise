using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace Prise.V2
{
    public interface IAssemblyLoadStrategy
    {
        /// <summary>
        /// Loads a dependency assembly for the current plugin
        /// </summary>
        /// <param name="fullPathToPluginAssembly">Full path to the corresponding plugin assembly</param>
        /// <param name="assemblyName"></param>
        /// <param name="loadFromDependencyContext"></param>
        /// <param name="loadFromRemote"></param>
        /// <param name="loadFromAppDomain"></param>
        /// <returns>A loaded assembly</returns>
        AssemblyFromStrategy LoadAssembly(string fullPathToPluginAssembly, AssemblyName assemblyName,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromDependencyContext,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromRemote,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromAppDomain);

        /// <summary>
        /// Loads a native assembly
        /// </summary>
        /// <param name="unmanagedDllName"></param>
        /// <param name="loadFromDependencyContext"></param>
        /// <param name="loadFromRemote"></param>
        /// <param name="loadFromAppDomain"></param>
        /// <returns>The path to a native assembly</returns>
        NativeAssembly LoadUnmanagedDll(string fullPathToPluginAssembly, string unmanagedDllName,
           Func<string, string, ValueOrProceed<string>> loadFromDependencyContext,
           Func<string, string, ValueOrProceed<string>> loadFromRemote,
           Func<string, string, ValueOrProceed<IntPtr>> loadFromAppDomain);
    }
}