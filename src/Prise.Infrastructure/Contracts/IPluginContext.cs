using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IPluginContext
    {
        Type PluginType { get; }
        Dictionary<Type, object> DependencyProvider { get; }
    }
}