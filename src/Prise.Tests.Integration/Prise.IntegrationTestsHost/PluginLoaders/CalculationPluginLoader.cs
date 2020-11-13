using Microsoft.AspNetCore.Http;
using Prise.IntegrationTestsContract;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System;
using Prise;

namespace Prise.IntegrationTestsHost.PluginLoaders
{
    public interface ICalculationPluginLoader
    {
        Task<ICalculationPlugin> GetPlugin();
        Task<IEnumerable<ICalculationPlugin>> GetPlugins();
    }

    public class CalculationPluginLoader : ICalculationPluginLoader
    {
        private readonly IPluginLoader pluginLoader;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly string pluginBaseDir = Path.GetFullPath("../../../../_dist", AppDomain.CurrentDomain.BaseDirectory);

        public CalculationPluginLoader(IPluginLoader pluginLoader, IHttpContextAccessor httpContextAccessor)
        {
            this.pluginLoader = pluginLoader;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<ICalculationPlugin> GetPlugin()
        {
            var pluginType = this.httpContextAccessor.HttpContext.Request.Headers["PluginType"].First();
            var plugins = await this.pluginLoader.FindPlugins<ICalculationPlugin>(Path.Combine(this.pluginBaseDir, pluginType));

            var firstPlugin = plugins.First();
            return await this.pluginLoader.LoadPlugin<ICalculationPlugin>(firstPlugin, configure: (ctx) =>
            {
                // ctx.
            });
        }

        public async Task<IEnumerable<ICalculationPlugin>> GetPlugins()
        {
            var pluginType = this.httpContextAccessor.HttpContext.Request.Headers["PluginType"].First();
            var plugins = await this.pluginLoader.FindPlugins<ICalculationPlugin>(Path.Combine(this.pluginBaseDir, pluginType));

            var instances = new List<ICalculationPlugin>();
            foreach (var plugin in plugins)
            {
                instances.Add(await this.pluginLoader.LoadPlugin<ICalculationPlugin>(plugin, configure: (ctx) =>
                {
                    // ctx.
                }));
            }
            return instances;
        }
    }
}