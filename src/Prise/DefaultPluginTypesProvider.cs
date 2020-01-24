using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Prise.Infrastructure;

namespace Prise
{
    public class DefaultPluginTypesProvider<T> : IPluginTypesProvider<T>
    {
        public IEnumerable<Type> ProvidePluginTypes(Assembly pluginAssembly)
        {
            return pluginAssembly
                            .GetTypes()
                            .Where(t => t.CustomAttributes
                                .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginAttribute).Name
                                && (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == typeof(T).Name))
                            .OrderBy(t => t.Name)
                            .AsEnumerable();
        }
    }
}
