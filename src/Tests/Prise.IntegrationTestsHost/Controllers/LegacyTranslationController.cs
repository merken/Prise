using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Prise.IntegrationTestsHost.Controllers
{
    [ApiController]
    [Route("translation/legacy")]
    public class LegacyTranslationController : ControllerBase
    {
        private readonly ILogger<LegacyTranslationController> logger;
        private readonly Legacy.Domain.ITranslationPlugin translationPlugin;

        public LegacyTranslationController(ILogger<LegacyTranslationController> logger, Legacy.Domain.ITranslationPlugin translationPlugin)
        {
            this.logger = logger;
            this.translationPlugin = translationPlugin;
        }

        [HttpGet]
        public async Task<IEnumerable<Legacy.Domain.TranslationOutput>> Translate([FromQuery] string input)
        {
            return await this.translationPlugin.Translate(new Legacy.Domain.TranslationInput { ContentToTranslate = input });
        }
    }
}
