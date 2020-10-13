using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Prise.AssemblyLoading
{
    public interface IAssemblyDependencyResolver : IDisposable
    {
        string ResolveAssemblyToPath(AssemblyName assemblyName);
        string ResolveUnmanagedDllToPath(string unmanagedDllName);
    }
}