using Contract;
using Microsoft.AspNetCore.Mvc;

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
        public string Get([FromQuery]string input)
        {
            return _helloWorldPlugin.SayHello(input);
        }
    }
}
