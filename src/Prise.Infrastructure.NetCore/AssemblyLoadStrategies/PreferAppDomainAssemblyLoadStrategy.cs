using System;
using System.Reflection;

namespace Prise.Infrastructure.NetCore
{
    internal class PreferAppDomainAssemblyLoadStrategy : AssemblyLoadStrategy, IAssemblyLoadStrategy
    {
        internal PreferAppDomainAssemblyLoadStrategy(IPluginDependencyContext pluginDependencyContext)
            : base(pluginDependencyContext) { }

        public Assembly LoadAssembly(AssemblyName assemblyName,
            Func<AssemblyName, ValueOrProceed<Assembly>> loadFromDependencyContext,
            Func<AssemblyName, ValueOrProceed<Assembly>> loadFromRemote,
            Func<AssemblyName, ValueOrProceed<Assembly>> loadFromAppDomain)
        {
            if (assemblyName.Name == null)
                return null;

            var valueOrProceed = TryLoadFromAppDomain(assemblyName, loadFromAppDomain);
            if (valueOrProceed.Value != null)
                return null; // fallback to default loading mechanism

            if (valueOrProceed.CanProceed)
                valueOrProceed = TryLoadFromDependencyContext(assemblyName, loadFromDependencyContext);

            if (valueOrProceed.CanProceed)
                valueOrProceed = TryLoadFromRemote(assemblyName, loadFromRemote);

            return valueOrProceed.Value;
        }

        public NativeAssembly LoadUnmanagedDll(string unmanagedDllName,
            Func<string, ValueOrProceed<string>> loadFromDependencyContext,
            Func<string, ValueOrProceed<string>> loadFromRemote,
            Func<string, ValueOrProceed<IntPtr>> loadFromAppDomain)
        {
            ValueOrProceed<string> valueOrProceed = ValueOrProceed<string>.FromValue(String.Empty, true);
            ValueOrProceed<IntPtr> ptrValueOrProceed = ValueOrProceed<IntPtr>.FromValue(IntPtr.Zero, true);

            ptrValueOrProceed = loadFromAppDomain(unmanagedDllName);

            if (ptrValueOrProceed.CanProceed)
                valueOrProceed = loadFromDependencyContext(unmanagedDllName);

            if (ptrValueOrProceed.CanProceed && valueOrProceed.CanProceed)
                valueOrProceed = loadFromRemote(unmanagedDllName);

            return NativeAssembly.Create(valueOrProceed.Value, ptrValueOrProceed.Value);
        }
    }
}
