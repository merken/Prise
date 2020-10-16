using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prise.IntegrationTestsContract;
using Prise.IntegrationTestsHost.PluginLoaders;
using Prise.IntegrationTestsHost.Models;
using System.Threading.Tasks;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("multiple")]
    public class MultipleCalculationController
    {
        private readonly ILogger<MultipleCalculationController> logger;
        private readonly ICalculationPluginLoader loader;

        public MultipleCalculationController(ILogger<MultipleCalculationController> logger, ICalculationPluginLoader loader)
        {
            this.logger = logger;
            this.loader = loader;
        }

        [HttpPost]
        public async Task<CalculationResponseModel> Calculate(CalculationRequestModel requestModel)
        {
            var plugins = await this.loader.GetPlugins();
            return new CalculationResponseModel
            {
                Result = plugins.Sum(p => p.Calculate(requestModel.A, requestModel.B))
            };
        }
    }
}