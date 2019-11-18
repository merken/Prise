using System;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IPluginLoader<T> : IDisposable
    {
        Task<T> Load();
        Task<T[]> LoadAll();
        Task Unload();
    }
}