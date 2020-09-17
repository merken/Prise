using System;
using System.Collections.Generic;
using System.Linq;

namespace Prise.Core
{
    public class DefaultPluginTypeSelector : IPluginTypeSelector
    {
        public IEnumerable<Type> SelectPluginTypes(Type type, IAssemblyShim assemblyShim)
        {
            return assemblyShim.Assembly
                            .GetTypes()
                            .Where(t => t.CustomAttributes
                                .Any(c => c.AttributeType.Name == typeof(Prise.Plugin.PluginAttribute).Name
                                && (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == type.Name))
                            .OrderBy(t => t.Name)
                            .AsEnumerable();
        }

        public IEnumerable<Type> SelectPluginTypes<T>(IAssemblyShim assemblyShim)
        {
            return this.SelectPluginTypes(typeof(T), assemblyShim);
        }
    }
}