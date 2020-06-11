using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IDowngradableDependenciesProvider<T> : IDisposable
    {
        IEnumerable<Type> ProvideDowngradableTypes();
        IEnumerable<string> ProvideDowngradableAssemblies();
    }
}
