using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<DiscoveryController> _logger;
        private readonly IPluginLoader<ICalculationPlugin> _loader;

        public DiscoveryController(ILogger<DiscoveryController> logger, IPluginLoader<ICalculationPlugin> loader)
        {
            _logger = logger;
            _loader = loader;
        }

        [HttpGet]
        public async Task<string> DiscoverPlugins()
        {
            var plugins = await _loader.LoadAll();

            return string.Join(',', plugins.Select(p => p.Name));
        }

        [HttpGet("description")]
        public async Task<string> DiscoverPluginsWithDescription()
        {
            var plugins = await _loader.LoadAll();

            return string.Join(',', plugins.Select(p => p.Description));
        }
    }
}
