using System;
using System.Collections.Generic;

namespace Prise.Infrastructure
{
    public interface IProbingPathsProvider : IDisposable
    {
        IEnumerable<string> GetProbingPaths();
    }
}
