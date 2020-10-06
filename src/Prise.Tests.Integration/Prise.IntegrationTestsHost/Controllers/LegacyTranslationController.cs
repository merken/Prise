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
        private readonly IEnumerable<Legacy.Domain.ITranslationPlugin> translationPlugins;

        public LegacyTranslationController(ILogger<LegacyTranslationController> logger, IEnumerable<Legacy.Domain.ITranslationPlugin> translationPlugins)
        {
            this.logger = logger;
            this.translationPlugins = translationPlugins;
        }

        [HttpGet]
        public async Task<IEnumerable<Legacy.Domain.TranslationOutput>> Translate([FromQuery] string input)
        {
            var results = new List<Legacy.Domain.TranslationOutput>();

            foreach (var plugin in translationPlugins)
                results.AddRange(await plugin.Translate(new Legacy.Domain.TranslationInput { ContentToTranslate = input }));

            return results;
        }
    }
}
