using System;
using System.Diagnostics;

namespace Prise.Infrastructure
{
    public interface IPluginAssemblyNameProvider<T> : IDisposable
    {
        string GetAssemblyName();
    }
}