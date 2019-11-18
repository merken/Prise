using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Contract;
using Prise;
using System.Text.Json;

namespace MyHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        private readonly IHelloWorldPlugin _helloWorldPlugin;

        public HelloController(IHelloWorldPlugin helloWorldPlugin)
        {
            _helloWorldPlugin = helloWorldPlugin;
        }

        [HttpGet]
        public async Task<string> Get([FromQuery]string language, [FromQuery]string input)
        {
            try
            {
                var result = await _helloWorldPlugin.SayHelloAsync(language, input);

                return result;
            }
            catch (AggregateException ex)
            {
                // u-oh, is the language not supported ?
            }

            var dictionary = await _helloWorldPlugin.GetHelloDictionaryAsync();
            var info = $"Language {language} is not supported. Supported languages are : {String.Join(',', dictionary.SupportedLanguages)}";
            return $"{info} \n {JsonSerializer.Serialize(dictionary)}";
        }
    }
}
