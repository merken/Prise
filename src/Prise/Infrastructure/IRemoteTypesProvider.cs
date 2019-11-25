using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IRemoteTypesProvider<T> : IDisposable
    {
        IEnumerable<Type> ProvideRemoteTypes();
    }
}
