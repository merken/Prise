using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Contract;
using Prise;
using Prise.Infrastructure;

namespace MyHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    /// <summary>
    /// To support lazy loading, you just need to inject the IPluginLoader
    /// </summary>
    public class LazyHelloController : ControllerBase
    {
        private readonly IPluginLoader<IHelloPlugin> _pluginLoader;

        public LazyHelloController(IPluginLoader<IHelloPlugin> pluginLoader)
        {
            _pluginLoader = pluginLoader;
        }

        [HttpGet]
        /// <summary>
        /// Load the plugin on-demand via the Load() or LoadAll() method
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<string> Get([FromQuery]string input)
        {
            var plugin = await _pluginLoader.Load();
            var response = plugin.SayHello(input);
            await _pluginLoader.Unload();
            return response;
        }
    }
}
