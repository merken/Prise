using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Contract;
using AppHost.Models;

namespace AppHost.Controllers
{
    [ApiController]
    [Route("multiple")]
    public class MultipleCalculationController
    {
        private readonly ILogger<MultipleCalculationController> _logger;
        private readonly IEnumerable<ICalculationPlugin> _plugins;

        // Multiple instances of plugins can be injected using an IEnumerable<TPlugin>
        public MultipleCalculationController(ILogger<MultipleCalculationController> logger, IEnumerable<ICalculationPlugin> plugins)
        {
            _logger = logger;
            _plugins = plugins;
        }

        [HttpPost]
        public CalculationResponseModel Calculate(CalculationRequestModel requestModel)
        {
            return new CalculationResponseModel
            {
                Result = _plugins.Sum(p => p.Calculate(requestModel.A, requestModel.B))
            };
        }
    }
}