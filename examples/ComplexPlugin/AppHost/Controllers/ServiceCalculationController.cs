using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using AppHost.Models;
using AppHost.Services;

namespace AppHost.Controllers
{
    [ApiController]
    [Route("service")]
    public class ServiceCalculationController : ControllerBase
    {
        private readonly ILogger<ServiceCalculationController> _logger;

        // Since the IPluginLoader is available through the DI container of .NET Core, a service can have it injectected, too!
        private readonly ICalculationService _service;

        public ServiceCalculationController(ILogger<ServiceCalculationController> logger, ICalculationService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        public Task<CalculationResponseModel> Calculate(CalculationRequestModel requestModel)
        {
            return _service.Calculate(requestModel);
        }

        [HttpPost("int")]
        public Task<CalculationResponseModel> CalculateInt(CalculationRequestModel requestModel)
        {
            return _service.CalculateInt(requestModel);
        }

        [HttpPost("complex-input")]
        public Task<CalculationResponseModel> CalculateComplex(CalculationRequestModel requestModel)
        {
            return _service.CalculateComplex(requestModel);
        }

        [HttpPost("complex-output")]
        public Task<CalculationResponseModel> CalculateComplexOutput(CalculationRequestModel requestModel)
        {
            return _service.CalculateComplexOutput(requestModel);
        }

        [HttpPost("multi")]
        public Task<CalculationResponseModel> CalculateMultiple(CalculationRequestMultiModel requestModel)
        {
            return _service.CalculateMultiple(requestModel);
        }
    }
}
