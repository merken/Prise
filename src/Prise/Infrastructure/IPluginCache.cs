using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Prise.Infrastructure
{
    public interface IPluginCache<T>
    {
        void Add(Assembly pluginAssembly);
        void Remove(string assemblyName);
        Assembly[] GetAll();
    }
}
