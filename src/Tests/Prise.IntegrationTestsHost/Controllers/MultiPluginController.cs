using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prise.IntegrationTestsContract;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("translation")]
    public class MultiPluginController : ControllerBase
    {
        private readonly ILogger<MultiPluginController> logger;
        private readonly ITokenService tokenService;
        private readonly IAuthenticatedDataService dataService;

        public MultiPluginController(
            ILogger<MultiPluginController> logger,
            ITokenService tokenService,
            IAuthenticatedDataService dataService)
        {
            this.logger = logger;
            this.tokenService = tokenService;
            this.dataService = dataService;
        }

        [HttpGet]
        public async Task<string> GetToken()
        {
            return await this.tokenService.GenerateToken();
        }

        [HttpGet("{token}")]
        public async Task<IEnumerable<Data>> GetData(string token)
        {
            return await this.dataService.GetData(token);
        }

    }
}
