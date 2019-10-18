using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace PluginServer.Custom
{
    public class ContextPluginAssemblyNameProvider : IPluginAssemblyNameProvider
    {
        private readonly IHttpContextAccessor contextAccessor;
        public ContextPluginAssemblyNameProvider(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public string GetAssemblyName()
        {
            var pluginType = this.contextAccessor.HttpContext.Request.Headers["PluginType"].First();
            
            return $"{pluginType}.dll";
        }
    }
}