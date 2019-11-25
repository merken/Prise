using System;
using System.IO;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IDepsFileProvider<T> : IDisposable
    {
        Task<Stream> ProvideDepsFile(IPluginLoadContext pluginLoadContext);
    }
}
