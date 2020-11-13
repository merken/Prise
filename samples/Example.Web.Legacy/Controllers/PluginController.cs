using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Example.Contract;
using Prise;

namespace Example.Web.Legacy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PluginController : ControllerBase
    {
        private readonly IPluginLoader pluginLoader;
        private readonly IHttpContextAccessorService httpContextAccessorService;
        private readonly IConfigurationService configurationService;

        public PluginController(IPluginLoader pluginLoader, IHttpContextAccessorService httpContextAccessorService, IConfigurationService configurationService)
        {
            this.pluginLoader = pluginLoader;
            this.httpContextAccessorService = httpContextAccessorService;
            this.configurationService = configurationService;
        }

        [HttpPost]
        public async Task<IActionResult> ExecutePlugin()
        {
            var pluginResults = await this.pluginLoader.FindPlugins<IPlugin>(GetPathToDist());
            var builder = new StringBuilder();

            foreach (var pluginResult in pluginResults)
            {
                try
                {
                    foreach (var plugin in await this.pluginLoader.LoadPlugins<IPlugin>(pluginResult,
                        configure: (context) =>
                        {
                            context.AddHostService<IHttpContextAccessorService>(this.httpContextAccessorService);
                            context.AddHostService<IConfigurationService>(this.configurationService);
                        }))
                    {
                        foreach (var data in await plugin.GetAll())
                            builder.AppendLine(data.Text);
                    }
                } catch (Exception ex) { builder.AppendLine(ex.Message); }
            }

            return new OkObjectResult(builder.ToString());
        }

        private static string GetPathToDist()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Plugins/_dist"));
        }
    }
}
