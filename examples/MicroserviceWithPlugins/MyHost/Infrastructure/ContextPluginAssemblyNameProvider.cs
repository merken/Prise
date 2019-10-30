using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace MyHost.Infrastructure
{
    public class ContextPluginAssemblyNameProvider : PluginAssemblyNameProvider
    {
        private readonly IHttpContextAccessor contextAccessor;
        public ContextPluginAssemblyNameProvider(IHttpContextAccessor contextAccessor)
            : base(String.Empty) // Pass an empty string to the base class because we don't know the assembly name yet
        {
            this.contextAccessor = contextAccessor;
        }

        public override string GetAssemblyName()
        {
            var pluginType = this.contextAccessor.HttpContext.Request.Headers["PluginType"].First();

            return $"{pluginType}.dll";
        }
    }
}