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
    [Route("eager")]
    public class EagerCalculationController : ControllerBase
    {
        private readonly ILogger<EagerCalculationController> _logger;
        private readonly ICalculationPlugin _plugin;

        public EagerCalculationController(ILogger<EagerCalculationController> logger, ICalculationPlugin plugin)
        {
            _logger = logger;
            _plugin = plugin;
        }

        [HttpPost]
        public CalculationResponseModel Calculate(CalculationRequestModel requestModel)
        {
            // The plugin is eagerly loaded (in-scope)
            return new CalculationResponseModel
            {
                Result = _plugin.Calculate(requestModel.A, requestModel.B)
            };
        }

        [HttpPost("int")]
        public CalculationResponseModel CalculateInt(CalculationRequestModel requestModel)
        {
            // Overloading works due to matching the Proxy on parameter count and types
            return new CalculationResponseModel
            {
                Result = _plugin.Calculate((int)requestModel.A, (int)requestModel.B)
            };
        }

        [HttpPost("complex-input")]
        public CalculationResponseModel CalculateComplex(CalculationRequestModel requestModel)
        {
            // Complex objects are serialized across Application Domains
            var context = new CalculationContext
            {
                A = requestModel.A,
                B = requestModel.B
            };
            return new CalculationResponseModel
            {
                Result = _plugin.CalculateComplex(context)
            };
        }

        [HttpPost("complex-output")]
        public CalculationResponseModel CalculateComplexOutput(CalculationRequestModel requestModel)
        {
            var context = new CalculationContext
            {
                A = requestModel.A,
                B = requestModel.B
            };
            // Complex results are dezerialized using Newtonsoft JSON (by default)
            return new CalculationResponseModel
            {
                Result = _plugin.CalculateComplexResult(context).Result
            };
        }

        [HttpPost("multi")]
        public CalculationResponseModel CalculateMultiple(CalculationRequestMultiModel requestModel)
        {
            // Ever more complex objects are serialized correctly
            var calculationContext = new ComplexCalculationContext
            {
                Calculations = requestModel.Calculations.Select(c => new CalculationContext { A = c.A, B = c.B }).ToArray()
            };

            return new CalculationResponseModel
            {
                Result = _plugin.CalculateMutiple(calculationContext).Results.Sum(r => r.Result)
            };
        }
    }
}
