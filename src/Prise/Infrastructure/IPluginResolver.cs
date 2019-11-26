using System;

namespace Prise.Infrastructure
{
    public interface IPluginResolver<out T> : IDisposable
    {
        T Load();
        T[] LoadAll();
        void Unload();
    }
}
