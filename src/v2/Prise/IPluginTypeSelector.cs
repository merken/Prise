using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using Prise.Proxy;

namespace Prise.V2
{
    public interface IPluginTypeSelector : IDisposable
    {
        IEnumerable<Type> SelectPluginTypes<T>(IAssemblyShim assemblyShim);
        IEnumerable<Type> SelectPluginTypes(Type type, IAssemblyShim assemblyShim);
    }

    public class DefaultPluginTypeSelector : IPluginTypeSelector
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

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