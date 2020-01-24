using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Prise.IntegrationTestsHost.Models;
using Prise.Infrastructure;
using Prise.IntegrationTestsContract;
using System.Linq;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("lazy")]
    public class LazyCalculationController : CalculationControllerBase
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
            base.SetPlugin(await _loader.Load());
            return base.Calculate(requestModel);
        }

        [HttpPost("int")]
        public async Task<CalculationResponseModel> CalculateInt(CalculationRequestModel requestModel)
        {
            base.SetPlugin(await _loader.Load());
            return base.CalculateInt(requestModel);
        }

        [HttpPost("complex-input")]
        public async Task<CalculationResponseModel> CalculateComplex(CalculationRequestModel requestModel)
        {
            base.SetPlugin(await _loader.Load());
            return base.CalculateComplex(requestModel);
        }

        [HttpPost("complex-output")]
        public async Task<CalculationResponseModel> CalculateComplexOutput(CalculationRequestModel requestModel)
        {
            base.SetPlugin(await _loader.Load());
            return base.CalculateComplexOutput(requestModel);
        }

        [HttpPost("multi")]
        public async Task<CalculationResponseModel> CalculateMultiple(CalculationRequestMultiModel requestModel)
        {
            base.SetPlugin(await _loader.Load());
            return base.CalculateMultiple(requestModel);
        }

        [HttpPost("multi-async")]
        public async Task<CalculationResponseModel> CalculateMultipleAsync(CalculationRequestMultiModel requestModel)
        {
            base.SetPlugin(await _loader.Load());
            return await base.CalculateMultipleAsync(requestModel);
        }
    }
}
