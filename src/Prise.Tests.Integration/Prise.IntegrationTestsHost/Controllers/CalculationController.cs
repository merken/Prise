using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Prise.IntegrationTestsHost.Models;
using Prise.IntegrationTestsContract;
using System.Threading.Tasks;
using Prise.IntegrationTestsHost.PluginLoaders;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("calculation")]
    public class CalculationController : CalculationControllerBase
    {
        private readonly ILogger<CalculationController> _logger;

        public CalculationController(ILogger<CalculationController> logger, ICalculationPluginLoader loader) : base(loader)
        {
            _logger = logger;
        }

        [HttpPost]
        public Task<CalculationResponseModel> Calculate(CalculationRequestModel requestModel) => base.Calculate(requestModel);

        [HttpPost("int")]
        public Task<CalculationResponseModel> CalculateInt(CalculationRequestModel requestModel) => base.CalculateInt(requestModel);

        [HttpPost("complex-input")]
        public Task<CalculationResponseModel> CalculateComplex(CalculationRequestModel requestModel) => base.CalculateComplex(requestModel);

        [HttpPost("complex-output")]
        public Task<CalculationResponseModel> CalculateComplexOutput(CalculationRequestModel requestModel) => base.CalculateComplexOutput(requestModel);

        [HttpPost("multi")]
        public Task<CalculationResponseModel> CalculateMultiple(CalculationRequestMultiModel requestModel) => base.CalculateMultiple(requestModel);

        [HttpPost("multi-async")]
        public Task<CalculationResponseModel> CalculateMultipleAsync(CalculationRequestMultiModel requestModel) => base.CalculateMultipleAsync(requestModel);
    }
}
