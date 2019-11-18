using System;
using System.Linq;
using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Plugin.Function.Infrastructure;
using Prise.Infrastructure;

namespace Plugin.Function
{
    public class PluginFunction
    {
        private readonly FunctionPluginLoader pluginLoader;

        public PluginFunction(IPluginLoader<IHelloPlugin> pluginLoader)
        {
            this.pluginLoader = pluginLoader as FunctionPluginLoader;
        }

        [FunctionName("ComponentFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var componentName = req.Query["component"].First();
                var input = req.Query["input"].First();

                this.pluginLoader.SetComponentToLoad(componentName);

                var plugin = await this.pluginLoader.Load();
                var result = await plugin.SayHello(input);
                return (ActionResult)new OkObjectResult(result);
            }
            catch (Exception ex)
            {
            }

            return null;

        }
    }
}