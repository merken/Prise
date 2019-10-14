using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using PluginServer.Models;
using Prise.Infrastructure;
using PluginContract;

namespace PluginServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : ControllerBase
    {
        private readonly ILogger<CalculationController> _logger;
        private readonly IPluginLoader<ICalculationPlugin> _loader;

        public CalculationController(ILogger<CalculationController> logger, IPluginLoader<ICalculationPlugin> loader)
        {
            _logger = logger;
            _loader = loader;
        }

        [HttpPost]
        public  async Task<CalculationResponseModel> Post(CalculationRequestModel requestModel)
        {
            // Load the plugin on-demand
            var plugin = await _loader.Load();
            return new CalculationResponseModel{
                Result = plugin.Calculate(requestModel.A, requestModel.B)
            };
        }

        [HttpPost("multi")]
        public CalculationResponseModel Postddd(CalculationRequestMultiModel requestModel)
        {
            return null;
        }
    }
}
