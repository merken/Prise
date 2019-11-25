using System;

namespace Prise.Infrastructure
{
    public interface IDependencyPathProvider<T> : IDisposable
    {
        string GetDependencyPath();
    }
}
