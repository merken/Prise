using Contract;
using Microsoft.AspNetCore.Mvc;
using Prise.Infrastructure;
using System.Threading.Tasks;

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
            return plugin.SayHello(input);
        }
    }
}
