using System;
using System.Diagnostics;

namespace Prise.Infrastructure
{
    public interface IPluginAssemblyNameProvider : IDisposable
    {
        string GetAssemblyName();
    }
}