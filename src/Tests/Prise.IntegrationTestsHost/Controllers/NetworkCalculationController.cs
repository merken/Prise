using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prise.IntegrationTestsContract;
using Prise.IntegrationTestsHost.Models;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("network")]
    public class NetworkCalculationController : CalculationControllerBase
    {
        private readonly ILogger<EagerCalculationController> _logger;

        public NetworkCalculationController(ILogger<EagerCalculationController> logger, INetworkCalculationPlugin plugin)
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
