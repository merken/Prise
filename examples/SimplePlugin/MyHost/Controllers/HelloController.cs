using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Contract;
using Prise.Infrastructure.NetCore;

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
