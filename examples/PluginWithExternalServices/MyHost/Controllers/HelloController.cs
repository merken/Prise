using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Contract;
using Prise;

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
