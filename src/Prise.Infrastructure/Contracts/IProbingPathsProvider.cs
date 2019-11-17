using System;
using System.Collections.Generic;
using System.Text;

namespace Prise.Infrastructure
{
    public interface IProbingPathsProvider : IDisposable
    {
        IEnumerable<string> GetProbingPaths();
    }
}
