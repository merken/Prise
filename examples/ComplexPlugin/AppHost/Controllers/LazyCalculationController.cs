using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using AppHost.Models;
using Prise.Infrastructure;
using Contract;
using System.Linq;

namespace AppHost.Controllers
{
    [ApiController]
    [Route("lazy")]
    public class LazyCalculationController : ControllerBase
    {
        private readonly ILogger<LazyCalculationController> _logger;
        private readonly IPluginLoader<ICalculationPlugin> _loader;

        public LazyCalculationController(ILogger<LazyCalculationController> logger, IPluginLoader<ICalculationPlugin> loader)
        {
            _logger = logger;
            _loader = loader;
        }

        [HttpPost]
        public async Task<CalculationResponseModel> Calculate(CalculationRequestModel requestModel)
        {
            // Load the plugin on-demand
            var plugin = await _loader.Load();
            return new CalculationResponseModel
            {
                Result = plugin.Calculate(requestModel.A, requestModel.B)
            };
        }

        [HttpPost("int")]
        public async Task<CalculationResponseModel> CalculateInt(CalculationRequestModel requestModel)
        {
            var plugin = await _loader.Load();
            // Overloading works due to matching the Proxy on parameter count and types
            return new CalculationResponseModel
            {
                Result = plugin.Calculate((int)requestModel.A, (int)requestModel.B)
            };
        }

        [HttpPost("complex-input")]
        public async Task<CalculationResponseModel> CalculateComplex(CalculationRequestModel requestModel)
        {
            var plugin = await _loader.Load();
            // Complex parameters are serialized across Application Domains using Newtonsoft JSON
            var context = new CalculationContext
            {
                A = requestModel.A,
                B = requestModel.B
            };
            return new CalculationResponseModel
            {
                Result = plugin.CalculateComplex(context)
            };
        }

        [HttpPost("complex-output")]
        public async Task<CalculationResponseModel> CalculateComplexOutput(CalculationRequestModel requestModel)
        {
            var plugin = await _loader.Load();
            var context = new CalculationContext
            {
                A = requestModel.A,
                B = requestModel.B
            };
            // Complex results are dezerialized using XML Deserialization (by default)
            return new CalculationResponseModel
            {
                Result = plugin.CalculateComplexResult(context).Result
            };
        }

        [HttpPost("multi")]
        public async Task<CalculationResponseModel> CalculateMultiple(CalculationRequestMultiModel requestModel)
        {
            var plugin = await _loader.Load();
            // Ever more complex objects are serialized correctly
            var calculationContext = new ComplexCalculationContext
            {
                Calculations = requestModel.Calculations.Select(c => new CalculationContext { A = c.A, B = c.B }).ToArray()
            };

            return new CalculationResponseModel
            {
                Result = plugin.CalculateMutiple(calculationContext).Results.Sum(r => r.Result)
            };
        }
    }
}
