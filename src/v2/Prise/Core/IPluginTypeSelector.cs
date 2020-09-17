using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Prise.Plugin;
using Prise.Proxy;

namespace Prise.Core
{
    public interface IPluginTypeSelector
    {
        IEnumerable<Type> SelectPluginTypes<T>(IAssemblyShim assemblyShim);
        IEnumerable<Type> SelectPluginTypes(Type type, IAssemblyShim assemblyShim);
    }
}