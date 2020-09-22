using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prise.Console.Contract;
using Prise.Web.Services;

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
    [Route("plugins")]
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
    [Route("storage-ondemand")]
    /// <summary>
    /// </summary>
    public class OnDemandStorageController : ControllerBase
    {
        private readonly IPluginLoader loader;

        public OnDemandStorageController(IPluginLoader loader)
        {
            this.loader = loader;
        }

        [HttpGet]
        public async Task<string> Get([FromQuery] string text)
        {
            var pluginResults = await this.loader.FindPlugins<IStoragePlugin>(GetPathToDist());
            if (!String.IsNullOrEmpty(text))
                pluginResults = pluginResults.Where(p => p.AssemblyPath.Split(Path.DirectorySeparatorChar).Last().Equals(text));

            if (!pluginResults.Any())
                return $"PLUGIN NOT FOUND {text}";

            var builder = new StringBuilder();

            foreach (var pluginResult in pluginResults)
            {
                var plugin = await this.loader.LoadPlugin<IStoragePlugin>(pluginResult);
                builder.AppendLine((await plugin.GetData(new PluginObject { Text = text })).Text);
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
