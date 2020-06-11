using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IHostTypesProvider<T> : IDisposable
    {
        IEnumerable<Type> ProvideHostTypes();
        IEnumerable<string> ProvideHostAssemblies();
    }
}
