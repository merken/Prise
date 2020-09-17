using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace Prise.V2
{

    public class ValueOrProceed<T>
    {
        public static ValueOrProceed<T> Proceed() => new ValueOrProceed<T>
        {
            CanProceed = true
        };

        public static ValueOrProceed<T> FromValue(T value, bool proceed) => new ValueOrProceed<T>
        {
            Value = value,
            CanProceed = proceed
        };

        public T Value { get; private set; }
        public bool CanProceed { get; private set; }
    }

    /// <summary>
    /// Represents a native library through either its full Path or a Pointer to an assembly in memory
    /// </summary>
    public class NativeAssembly
    {
        public string Path { get; private set; }
        public IntPtr Pointer { get; private set; }

        public static NativeAssembly Create(string path, IntPtr pointer) => new NativeAssembly { Path = path, Pointer = pointer };
    }

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

    public interface INativeAssemblyUnloader
    {
        void UnloadNativeAssembly(string fullPathToLoadedNativeAssembly, IntPtr pointerToAssembly);
    }

    public interface IAssemblyLoadContext : IDisposable
    {
        Task<IAssemblyShim> LoadPluginAssembly(IPluginLoadContext loadContext);

        Task Unload();
    }

    [Serializable]
    public class AssemblyLoadException : Exception
    {
        public AssemblyLoadException(string message) : base(message)
        {
        }

        public AssemblyLoadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AssemblyLoadException()
        {
        }

        protected AssemblyLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class DefaultAssemblyLoadStrategy : IAssemblyLoadStrategy
    {
        protected IPluginDependencyContext pluginDependencyContext;

        public DefaultAssemblyLoadStrategy(IPluginDependencyContext pluginDependencyContext)
        {
            this.pluginDependencyContext = pluginDependencyContext;
        }

        public virtual AssemblyFromStrategy LoadAssembly(string fullPathToPluginAssembly, AssemblyName assemblyName,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromDependencyContext,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromRemote,
            Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>> loadFromAppDomain)
        {
            if (assemblyName.Name == null)
                return null;

            ValueOrProceed<AssemblyFromStrategy> valueOrProceed = ValueOrProceed<AssemblyFromStrategy>.FromValue(null, true);

            var isHostAssembly = IsHostAssembly(assemblyName);
            var isRemoteAssembly = IsRemoteAssembly(assemblyName);

            if (isHostAssembly && !isRemoteAssembly) // Load from Default App Domain (host)
            {
                valueOrProceed = loadFromAppDomain(fullPathToPluginAssembly, assemblyName);
                if (valueOrProceed.Value != null)
                    return null; // fallback to default loading mechanism
            }

            if (valueOrProceed.CanProceed)

                valueOrProceed = loadFromDependencyContext(fullPathToPluginAssembly, assemblyName);


            if (valueOrProceed.CanProceed)
                valueOrProceed = loadFromRemote(fullPathToPluginAssembly, assemblyName);

            return valueOrProceed.Value;
        }

        public virtual NativeAssembly LoadUnmanagedDll(string fullPathToPluginAssembly, string unmanagedDllName,
            Func<string, string, ValueOrProceed<string>> loadFromDependencyContext,
            Func<string, string, ValueOrProceed<string>> loadFromRemote,
            Func<string, string, ValueOrProceed<IntPtr>> loadFromAppDomain)
        {
            ValueOrProceed<string> valueOrProceed = ValueOrProceed<string>.FromValue(String.Empty, true);
            ValueOrProceed<IntPtr> ptrValueOrProceed = ValueOrProceed<IntPtr>.FromValue(IntPtr.Zero, true);

            valueOrProceed = loadFromDependencyContext(fullPathToPluginAssembly, unmanagedDllName);

            if (valueOrProceed.CanProceed)
                ptrValueOrProceed = loadFromAppDomain(fullPathToPluginAssembly, unmanagedDllName);

            if (valueOrProceed.CanProceed && ptrValueOrProceed.CanProceed)
                valueOrProceed = loadFromRemote(fullPathToPluginAssembly, unmanagedDllName);

            return NativeAssembly.Create(valueOrProceed.Value, ptrValueOrProceed.Value);
        }

        protected virtual bool IsHostAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.HostDependencies.Any(h => h.DependencyName.Name == assemblyName.Name);
        protected virtual bool IsRemoteAssembly(AssemblyName assemblyName) => this.pluginDependencyContext.RemoteDependencies.Any(r => r.DependencyName.Name == assemblyName.Name);
    }
}