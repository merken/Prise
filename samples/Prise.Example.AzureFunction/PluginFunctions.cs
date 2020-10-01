using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Prise.Example.Contract;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Prise.Core;

namespace Prise.Example.AzureFunction
{
    public class PluginFunctions
    {
        private readonly IPluginLoader pluginLoader;
        private readonly IConfigurationService configurationService;

        public PluginFunctions(IPluginLoader pluginLoader, IConfigurationService configurationService)
        {
            this.pluginLoader = pluginLoader;
            this.configurationService = configurationService;
        }

        [FunctionName("PluginFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var pluginToLoad = req.Query["plugin"];
            var dist = GetPathToDist();

            var pluginScanResult = await this.pluginLoader.FindPlugin<IPlugin>(dist, pluginToLoad);
            if (pluginScanResult == null)
                return (ActionResult)new BadRequestObjectResult($"Plugin not found: {pluginToLoad}");

            var plugin = await this.pluginLoader.LoadPlugin<IPlugin>(pluginScanResult, (context) =>
                    {
                        context.AddHostService<IConfigurationService>(this.configurationService);
                    });

            var builder = new StringBuilder();
            foreach (var result in await plugin.GetAll())
                builder.AppendLine($"{result.Text}");

            return (ActionResult)new OkObjectResult(builder.ToString());
        }

        private static string GetPathToDist()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../../Plugins/dist"));
        }
    }
}