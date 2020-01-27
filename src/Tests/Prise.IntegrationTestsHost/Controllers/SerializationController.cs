using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Prise.Infrastructure;
using Prise.IntegrationTestsContract;

namespace Prise.IntegrationTestsHost.Controllers
{
    class HostObject
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
        public double DoubleProperty { get; set; }
    }

    public class SerializationPluginSelector : IPluginSelector<IPluginWithSerializer>
    {
        private readonly IHttpContextAccessor contextAccessor;
        public SerializationPluginSelector(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public IEnumerable<Type> SelectPlugins(IEnumerable<Type> pluginTypes)
        {
            if (!this.contextAccessor.HttpContext.Request.Headers.ContainsKey(HeaderNames.Accept))
                throw new NotSupportedException("Please set a value for the Accept Http Header");

            var accept = this.contextAccessor.HttpContext.Request.Headers[HeaderNames.Accept];
            var acceptValue = accept[0];

            if (acceptValue == "application/json")
                return pluginTypes.Where(p => p.Name == "JsonSerializerPlugin");
            if (acceptValue == "application/xml")
                return pluginTypes.Where(p => p.Name == "XmlSerializerPlugin");

            return pluginTypes;
        }
    }

    [ApiController]
    [Route("serialization")]
    public class SerializationController : ControllerBase
    {
        private readonly IPluginWithSerializer plugin;
        public SerializationController(IPluginWithSerializer plugin)
        {
            this.plugin = plugin;
        }

        [HttpGet]
        public string Serialize()
        {
            var hostObject = new ObjectToSerialize
            {
                StringProperty = "Some string",
                IntProperty = 999,
                DoubleProperty = 4336.99d
            };

            try
            {
                return this.plugin.SerializeObject(hostObject);
            }
            catch (NotSupportedException nex)
            {
                return nex.Message;
            }
        }
    }
}
