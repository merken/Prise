using System.Collections.Concurrent;
using System.Reflection;

namespace Prise
{
    public interface INetworkAssemblyCache
    {
        void AddOrReplace(Assembly assembly);
        Assembly TryGet(AssemblyName assemblyName);
        Assembly TryGet(string assemblyName);
    }

    public class NetworkAssemblyCache : INetworkAssemblyCache
    {
        private ConcurrentDictionary<string, Assembly> cache;

        public NetworkAssemblyCache()
        {
            this.cache = new ConcurrentDictionary<string, Assembly>();
        }

        public void AddOrReplace(Assembly assembly)
        {
            this.cache[assembly.GetName().Name] = assembly;
        }

        public Assembly TryGet(AssemblyName assemblyName)
        {
            return TryGet(assemblyName.Name);
        }

        public Assembly TryGet(string assemblyName)
        {
            if (this.cache.ContainsKey(assemblyName))
                return this.cache[assemblyName];

            return null;
        }
    }
}
