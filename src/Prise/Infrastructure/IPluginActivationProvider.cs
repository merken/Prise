using System;
using System.Collections.Generic;
using System.Reflection;

namespace Prise.Infrastructure
{
    public interface IPluginTypesProvider<T>
    {
        IEnumerable<Type> ProvidePluginTypes(Assembly pluginAssembly);
    }
}
