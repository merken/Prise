using System;

namespace Prise.Infrastructure.NetCore
{
    public interface ITempPathProvider : IDisposable
    {
        string ProvideTempPath(string assemblyName);
    }
}
