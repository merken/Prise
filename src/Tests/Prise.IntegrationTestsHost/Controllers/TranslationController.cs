using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prise.IntegrationTestsContract;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("translation")]
    public class TranslationController : ControllerBase
    {
        private readonly ILogger<TranslationController> logger;
        private readonly ITranslationPlugin translationPlugin;

        public TranslationController(ILogger<TranslationController> logger, ITranslationPlugin translationPlugin)
        {
            this.logger = logger;
            this.translationPlugin = translationPlugin;
        }

        [HttpGet]
        public async Task<IEnumerable<TranslationOutput>> Translate([FromQuery] string input)
        {
            return await this.translationPlugin.Translate(new TranslationInput { ContentToTranslate = input });
        }
    }
}
