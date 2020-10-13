using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Prise.Caching
{
    public class CachedPluginAssembly : ICachedPluginAssembly
    {
        public IAssemblyShim AssemblyShim { get; set; }

        public IEnumerable<Type> HostTypes { get; set; }
    }
}