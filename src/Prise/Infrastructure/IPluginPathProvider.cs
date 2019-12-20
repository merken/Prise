using System;

namespace Prise.Infrastructure
{
    public interface IPluginPathProvider<T> : IDisposable
    {
        string GetPluginPath();
    }
}
