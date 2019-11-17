using System;
using System.Collections.Generic;
using System.Text;

namespace Prise.Infrastructure
{
    public interface IHostTypesProvider : IDisposable
    {
        IEnumerable<Type> ProvideHostTypes();
    }
}
