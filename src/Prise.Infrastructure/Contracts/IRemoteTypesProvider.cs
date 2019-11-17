using System;
using System.Collections.Generic;
using System.Text;

namespace Prise.Infrastructure
{
    public interface IRemoteTypesProvider : IDisposable
    {
        IEnumerable<Type> ProvideRemoteTypes();
    }
}
