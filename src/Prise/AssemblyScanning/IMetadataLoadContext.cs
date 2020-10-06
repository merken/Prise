using System;
using System.Reflection;

namespace Prise.AssemblyScanning
{
    public interface IMetadataLoadContext : IDisposable
    {
        IAssemblyShim LoadFromAssemblyName(string assemblyName);
    }
}