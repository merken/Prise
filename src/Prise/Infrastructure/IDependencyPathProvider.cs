using System;

namespace Prise.Infrastructure
{
    public interface IDependencyPathProvider : IDisposable
    {
        string GetDependencyPath();
    }
}
