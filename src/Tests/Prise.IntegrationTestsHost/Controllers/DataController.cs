using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prise.IntegrationTestsContract;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("data")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> logger;
        private readonly ITokenService tokenService;
        private readonly IAuthenticatedDataService dataService;
        private readonly ICalculationPlugin calculationPlugin;
#if NETCORE3_0 || NETCORE3_1
        private readonly ITranslationPlugin translationPlugin;
#endif
        public DataController(
            ILogger<DataController> logger,
            ITokenService tokenService,
            IAuthenticatedDataService dataService,
#if NETCORE3_0 || NETCORE3_1
            ITranslationPlugin translationPlugin,
#endif
            ICalculationPlugin calculationPlugin
            )
        {
            this.logger = logger;
            this.tokenService = tokenService;
            this.dataService = dataService;
#if NETCORE3_0 || NETCORE3_1
            this.translationPlugin = translationPlugin;
#endif
            this.calculationPlugin = calculationPlugin;
        }

        [HttpGet]
        public async Task<string> GetToken()
        {
            return await this.tokenService.GenerateToken();
        }

#if NETCORE3_0 || NETCORE3_1
        [HttpGet("{token}")]
        public async Task<IEnumerable<Data>> GetData(string token)
        {
            await this.calculationPlugin.CalculateMutipleAsync(new ComplexCalculationContext { Calculations = new[] { new CalculationContext { A = 10, B = 15 } } });
            await this.translationPlugin.Translate(new TranslationInput { ContentToTranslate = "dog" });
            return await this.dataService.GetData(token);
        }
#endif

        [HttpGet("{token}/V1")]
        public async Task<IEnumerable<Data>> GetDataV1(string token)
        {
            await this.calculationPlugin.CalculateMutipleAsync(new ComplexCalculationContext { Calculations = new[] { new CalculationContext { A = 10, B = 15 } } });
            return await this.dataService.GetData(token);
        }
    }
}
