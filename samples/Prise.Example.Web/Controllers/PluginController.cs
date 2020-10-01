using System.IO;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prise.Example.Contract;
using Prise.Core;

namespace Prise.Example.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PluginController : ControllerBase
    {
        private readonly IPluginLoader pluginLoader;
        private readonly IHttpContextAccessorService httpContextAccessorService;

        public PluginController(IPluginLoader pluginLoader, IHttpContextAccessorService httpContextAccessorService)
        {
            this.pluginLoader = pluginLoader;
            this.httpContextAccessorService = httpContextAccessorService;
        }

        [HttpPost]
        public async Task<IActionResult> ExecutePlugin()
        {
            var pluginResults = await this.pluginLoader.FindPlugins<IPlugin>(GetPathToDist());
            var builder = new StringBuilder();

            foreach (var pluginResult in pluginResults)
            {
                await foreach (var plugin in this.pluginLoader.LoadPlugins<IPlugin>(pluginResult,
                    (context) =>
                    {
                        context.AddHostService<IHttpContextAccessorService>(this.httpContextAccessorService);
                    }))
                {
                    foreach (var data in await plugin.GetAll())
                        builder.AppendLine(data.Text);
                }
            }

            return new OkObjectResult(builder.ToString());
        }

        private static string GetPathToDist()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Plugins/dist"));
        }
    }
}
