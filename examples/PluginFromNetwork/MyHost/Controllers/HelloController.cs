using Contract;
using Microsoft.AspNetCore.Mvc;

namespace MyHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        private readonly IHelloPlugin _helloPlugin;

        public HelloController(IHelloPlugin helloPlugin)
        {
            _helloPlugin = helloPlugin;
        }

        [HttpGet]
        public string Get([FromQuery]string input)
        {
            return _helloPlugin.SayHello(input);
        }
    }
}
