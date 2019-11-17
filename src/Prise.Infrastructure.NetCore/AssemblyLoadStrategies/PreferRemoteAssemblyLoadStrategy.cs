using System;
using System.Reflection;

namespace Prise.Infrastructure.NetCore
{
    internal class PreferRemoteAssemblyLoadStrategy : AssemblyLoadStrategy, IAssemblyLoadStrategy
    {
        internal PreferRemoteAssemblyLoadStrategy(IPluginDependencyContext pluginDependencyContext)
            : base(pluginDependencyContext) { }

        public Assembly LoadAssembly(AssemblyName assemblyName,
            Func<AssemblyName, ValueOrProceed<Assembly>> loadFromDependencyContext,
            Func<AssemblyName, ValueOrProceed<Assembly>> loadFromRemote,
            Func<AssemblyName, ValueOrProceed<Assembly>> loadFromAppDomain)
        {
            if (assemblyName.Name == null)
                return null;

            ValueOrProceed<Assembly> valueOrProceed = ValueOrProceed<Assembly>.FromValue(null, true);

            valueOrProceed = TryLoadFromRemote(assemblyName, loadFromRemote);

            if (valueOrProceed.CanProceed)
                valueOrProceed = TryLoadFromDependencyContext(assemblyName, loadFromDependencyContext);

            if (valueOrProceed.CanProceed)
                valueOrProceed = TryLoadFromAppDomain(assemblyName, loadFromAppDomain);

            return valueOrProceed.Value;
        }

        public NativeAssembly LoadUnmanagedDll(string unmanagedDllName,
            Func<string, ValueOrProceed<string>> loadFromDependencyContext,
            Func<string, ValueOrProceed<string>> loadFromRemote,
            Func<string, ValueOrProceed<IntPtr>> loadFromAppDomain)
        {
            ValueOrProceed<string> valueOrProceed = ValueOrProceed<string>.FromValue(String.Empty, true);
            ValueOrProceed<IntPtr> ptrValueOrProceed = ValueOrProceed<IntPtr>.FromValue(IntPtr.Zero, true);

            valueOrProceed = loadFromRemote(unmanagedDllName);

            if (valueOrProceed.CanProceed)
                valueOrProceed = loadFromDependencyContext(unmanagedDllName);

            if (valueOrProceed.CanProceed)
                ptrValueOrProceed = loadFromAppDomain(unmanagedDllName);

            return NativeAssembly.Create(valueOrProceed.Value, ptrValueOrProceed.Value);
        }
    }
}
