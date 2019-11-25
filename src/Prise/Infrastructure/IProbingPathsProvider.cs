using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IProbingPathsProvider<T> : IDisposable
    {
        IEnumerable<string> GetProbingPaths();
    }
}
