using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Prise.Console.Contract;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Prise.Functions.Example
{
    public class PluginFunctions
    {
        private readonly IPluginLoader pluginLoader;

        public PluginFunctions(IPluginLoader pluginLoader)
        {
            this.pluginLoader = pluginLoader;
        }

        [FunctionName("PluginFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var pluginToLoad = req.Query["plugin"];
            var dist = GetPathToDist();

            var pluginScanResult = await this.pluginLoader.FindPlugin<IStoragePlugin>(dist, pluginToLoad);
            if (pluginScanResult == null)
                return (ActionResult)new BadRequestObjectResult($"NOT FOUND {pluginToLoad}");
            var plugin = await this.pluginLoader.LoadPlugin<IStoragePlugin>(pluginScanResult);

            var result = (await plugin.GetData(new PluginObject())).Text;

            return (ActionResult)new OkObjectResult(result);
        }

        private static string GetPathToDist()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../../Packages/dist"));
        }
    }
}
