using Prise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyHost.Infrastructure
{
    public static class PluginLoadOptionsBuilderExtensions
    {
        public static PluginLoadOptionsBuilder<T> LocalOrNetwork<T>(this PluginLoadOptionsBuilder<T> optionsBuilder, bool network = false)
        {
            if (network)
            {
                return optionsBuilder.WithNetworkAssemblyLoader<TenantPluginNetworkLoadOptions<T>>();
            }
            return optionsBuilder.WithLocalDiskAssemblyLoader();
        }
    }
}
