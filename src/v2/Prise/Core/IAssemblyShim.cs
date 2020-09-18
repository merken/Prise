using System;
using System.Collections.Generic;
using System.Reflection;

namespace Prise
{
    public interface IAssemblyShim
    {
        Assembly Assembly { get; }
        IEnumerable<Type> Types { get; }
    }
}