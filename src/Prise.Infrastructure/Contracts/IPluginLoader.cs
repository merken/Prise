using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IPluginLoader<T>
    {
        Task<T> Load();
        Task<T[]> LoadAll();
        Task Unload();
    }
}