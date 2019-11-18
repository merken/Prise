using System;

namespace Prise.Infrastructure
{
    public interface ITempPathProvider : IDisposable
    {
        string ProvideTempPath(string assemblyName);
    }
}
