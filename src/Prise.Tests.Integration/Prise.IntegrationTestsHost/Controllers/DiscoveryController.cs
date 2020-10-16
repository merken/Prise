using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Prise.IntegrationTestsHost.PluginLoaders;
using Prise.IntegrationTestsHost.Models;
using Prise.IntegrationTestsContract;
using Prise.Infrastructure;
using System.Text;
using System.Threading.Tasks;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("disco")]
    public class DiscoveryController : ControllerBase
    {
        private readonly ILogger<DiscoveryController> logger;
        private readonly ICalculationPluginLoader loader;

        public DiscoveryController(ILogger<DiscoveryController> logger, ICalculationPluginLoader loader)
        {
            this.logger = logger;
            this.loader = loader;
        }

        [HttpGet]
        public async Task<string> DiscoverPlugins()
        {
            var plugins = await this.loader.GetPlugins();

            return string.Join(',', plugins.Select(p => p.Name));
        }

        [HttpGet("description")]
        public async Task<string> DiscoverPluginsWithDescription()
        {
            var plugins = await this.loader.GetPlugins();

            return string.Join(',', plugins.Select(p => p.Description));
        }
    }
}
