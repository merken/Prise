using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Prise.IntegrationTestsHost.Models;
using Prise.IntegrationTestsContract;
using System.Threading.Tasks;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("eager")]
    public class EagerCalculationController : CalculationControllerBase
    {
        private readonly ILogger<EagerCalculationController> _logger;

        public EagerCalculationController(ILogger<EagerCalculationController> logger, ICalculationPlugin plugin)
        {
            _logger = logger;
            base.SetPlugin(plugin);
        }

        [HttpPost]
        public CalculationResponseModel Calculate(CalculationRequestModel requestModel) => base.Calculate(requestModel);

        [HttpPost("int")]
        public CalculationResponseModel CalculateInt(CalculationRequestModel requestModel) => base.CalculateInt(requestModel);

        [HttpPost("complex-input")]
        public CalculationResponseModel CalculateComplex(CalculationRequestModel requestModel) => base.CalculateComplex(requestModel);

        [HttpPost("complex-output")]
        public CalculationResponseModel CalculateComplexOutput(CalculationRequestModel requestModel) => base.CalculateComplexOutput(requestModel);

        [HttpPost("multi")]
        public CalculationResponseModel CalculateMultiple(CalculationRequestMultiModel requestModel) => base.CalculateMultiple(requestModel);

        [HttpPost("multi-async")]
        public Task<CalculationResponseModel> CalculateMultipleAsync(CalculationRequestMultiModel requestModel) => base.CalculateMultipleAsync(requestModel);
    }
}
