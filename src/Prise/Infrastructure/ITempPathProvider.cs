using System;

namespace Prise.Infrastructure
{
    public interface ITempPathProvider<T> : IDisposable
    {
        string ProvideTempPath(string assemblyName);
    }
}
