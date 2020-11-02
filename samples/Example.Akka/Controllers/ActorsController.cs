using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Example.Contract;
using Example.Akka.Domain;
using Akka.Actor;

namespace Example.Akka.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ActorsController : ControllerBase
    {
        private readonly ILogger<ActorsController> logger;
        private readonly IActorRef actorRef;

        public ActorsController(ILogger<ActorsController> logger, IPluginsActorProvider pluginsActorProvider)
        {
            this.logger = logger;
            this.actorRef = pluginsActorProvider.ProvideActor();
        }

        [HttpGet("{plugin}")]
        public async Task<IEnumerable<MyDto>> Get(string plugin)
        {
            var results = await actorRef.Ask<IEnumerable<MyDto>>(new GetAllCommand()
            {
                Plugin = plugin
            });
            return results;
        }
    }
}
