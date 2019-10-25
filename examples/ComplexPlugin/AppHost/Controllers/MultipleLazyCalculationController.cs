using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Contract;
using AppHost.Models;
using Prise.Infrastructure;

namespace AppHost.Controllers
{
    [ApiController]
    [Route("multiple-lazy")]
    public class MultipleLazyCalculationController
    {
        private readonly ILogger<MultipleLazyCalculationController> _logger;
        private readonly IPluginLoader<ICalculationPlugin> _pluginLoader;

        // Multiple instances of plugins can be loaded using the IPluginLoader
        public MultipleLazyCalculationController(ILogger<MultipleLazyCalculationController> logger, IPluginLoader<ICalculationPlugin> pluginLoader)
        {
            _logger = logger;
            _pluginLoader = pluginLoader;
        }

        [HttpPost]
        public async Task<CalculationResponseModel> Calculate(CalculationRequestModel requestModel)
        {
            var _plugins = await _pluginLoader.LoadAll();
            return new CalculationResponseModel
            {
                Result = _plugins.Sum(p => p.Calculate(requestModel.A, requestModel.B))
            };
        }
    }
}