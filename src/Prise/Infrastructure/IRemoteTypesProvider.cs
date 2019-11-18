using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IRemoteTypesProvider : IDisposable
    {
        IEnumerable<Type> ProvideRemoteTypes();
    }
}
