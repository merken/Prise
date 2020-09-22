using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prise.Console.Contract;

namespace Prise.Web.Controllers
{
    [ApiController]
    [Route("plugin")]
    /// <summary>
    /// This controller will have the IPlugin plugin injected automatically.
    /// Using this, there can only be 1 plugin registered and resolved from 1 directory
    /// You could replace the plugin in the directory with any other plugin at runtime, but there can still be only 1
    /// </summary>
    public class InjectionController : ControllerBase
    {
        private readonly IPlugin plugin;

        public InjectionController(IPlugin plugin)
        {
            this.plugin = plugin;
        }

        [HttpGet]
        public async Task<PluginObject> Get([FromQuery] string text)
        {
            return await this.plugin.GetData(new PluginObject
            {
                Number = new Random().Next(),
                Text = text
            });
        }
    }

    [ApiController]
    [Route("plugddins")]
    /// <summary>
    /// This controller will have the IPlugin plugin injected automatically.
    /// Using this, there can only be 1 plugin registered and resolved from 1 directory
    /// You could replace the plugin in the directory with any other plugin at runtime, but there can still be only 1
    /// </summary>
    public class InjectionMultipleController : ControllerBase
    {
        private readonly IEnumerable<IMultiplePlugin> plugins;

        public InjectionMultipleController(IEnumerable<IMultiplePlugin> plugins)
        {
            this.plugins = plugins;
        }

        [HttpGet]
        public async Task<string> Get([FromQuery] string text)
        {
            var builder = new StringBuilder();

            foreach (var plugin in this.plugins)
                builder.AppendLine((await plugin.GetData(new PluginObject { Text = text })).Text);

            return builder.ToString();
        }
    }

    [ApiController]
    [Route("storage")]
    /// <summary>
    /// </summary>
    public class StorageController : ControllerBase
    {
        private readonly IEnumerable<IStoragePlugin> plugins;

        public StorageController(IEnumerable<IStoragePlugin> plugins)
        {
            this.plugins = plugins;
        }

        [HttpGet]
        public async Task<string> Get([FromQuery] string text)
        {
            var builder = new StringBuilder();

            foreach (var plugin in this.plugins)
                builder.AppendLine((await plugin.GetData(new PluginObject { Text = text })).Text);

            return builder.ToString();
        }
    }

    [ApiController]
    [Route("plugins")]
    /// <summary>
    /// </summary>
    public class PluginController : ControllerBase
    {
        private readonly IPluginLoader loader;

        public PluginController(IPluginLoader loader)
        {
            this.loader = loader;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var pluginResults = await this.loader.FindPlugins<IPlugin>(GetPathToDist());
            var builder = new StringBuilder();

            foreach (var pluginResult in pluginResults)
            {
                await foreach (var plugin in this.loader.LoadPlugins<IPlugin>(pluginResult)){
                    builder.AppendLine((await plugin.GetData(new PluginObject { Text = "" })).Text);
                    try{
                    builder.AppendLine((await plugin.MyNewMethod(new PluginObject { Text = "" })).Text);
                    }catch(Prise.Proxy.PriseProxyException pex){
                        builder.AppendLine("This is an older plugin");
                    }
                }
            }

            return builder.ToString();
        }

        private static string GetPathToDist()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist"));
        }
    }
}
