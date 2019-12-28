using Prise.Infrastructure;
using System;
using System.Reflection;

namespace Prise.Tests.DefaultAssemblyLoadStrategy
{
    public class DefaultAssemblyLoadStrategyBase : TestBase
    {
        // You cannot create an instance of the Abstract Assembly class, instead of constructing a valid Assembly, return the entry assembly from the unit test
        protected Assembly GetRealAssembly() => System.Reflection.Assembly.GetEntryAssembly();

        protected Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> CreateLookupFunction(Func<IPluginLoadContext, AssemblyName, ValueOrProceed<Assembly>> func) => func;
        protected Func<IPluginLoadContext, T1, ValueOrProceed<T2>> CreateLookupFunction<T1, T2>(Func<IPluginLoadContext, T1, ValueOrProceed<T2>> func) => func;
    }
}
