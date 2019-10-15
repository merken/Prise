using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using PluginServer.Models;
using Prise.Infrastructure;
using PluginContract;
using PluginServer.Services;

namespace PluginServer.Controllers
{
    [ApiController]
    [Route("service")]
    public class ServiceCalculationController : ControllerBase
    {
        private readonly ILogger<ServiceCalculationController> _logger;
        private readonly ICalculationService _service;

        public ServiceCalculationController(ILogger<ServiceCalculationController> logger, ICalculationService service)
        {
            _logger = logger;
            this._service = service;
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
