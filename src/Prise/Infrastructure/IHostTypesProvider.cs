using System;
using System.Collections.Generic;
using System.Text;

namespace Prise.Infrastructure
{
    public interface IHostTypesProvider<T> : IDisposable
    {
        IEnumerable<Type> ProvideHostTypes();
    }
}
