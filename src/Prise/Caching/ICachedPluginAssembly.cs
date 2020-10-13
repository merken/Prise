using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Prise.Caching
{
    public interface ICachedPluginAssembly
    {
        IAssemblyShim AssemblyShim { get; }
        IEnumerable<Type> HostTypes { get; }
    }
}