using System;
using System.Collections.Generic;
using System.Text;

namespace Prise.Infrastructure
{
    public interface IPluginResolver<out T> : IDisposable
    {
        T Load();
        T[] LoadAll();
        void Unload();
    }
}
