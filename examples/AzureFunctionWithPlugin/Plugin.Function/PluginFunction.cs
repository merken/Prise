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
        private readonly FunctionPluginLoaderOptions pluginLoadOptions;

        public PluginFunction(FunctionPluginLoaderOptions pluginLoadOptions)
        {
            this.pluginLoadOptions = pluginLoadOptions;
        }

        [FunctionName("ComponentFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var componentName = req.Query["component"].First();
            var input = req.Query["input"].First();
            using (var pluginLoader = this.pluginLoadOptions.CreateLoaderForComponent(componentName))
            {
                var plugin = await pluginLoader.Load();
                var result = await plugin.SayHello(input);
                return (ActionResult)new OkObjectResult(result);
            }
        }
    }
}