using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace Prise.V2
{
    public interface IAssemblyLoadContext : IDisposable
    {
        Task<IAssemblyShim> LoadPluginAssembly(IPluginLoadContext loadContext);

        Task Unload();
    }
}