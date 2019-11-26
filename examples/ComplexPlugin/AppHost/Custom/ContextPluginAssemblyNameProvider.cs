using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise;

namespace AppHost.Custom
{
    public class ContextPluginAssemblyNameProvider<T> : PluginAssemblyNameProvider<T>
    {
        private readonly IHttpContextAccessor contextAccessor;
        public ContextPluginAssemblyNameProvider(IHttpContextAccessor contextAccessor)
            : base(String.Empty) // Pass an empty string to the base class because we don't know the assembly name yet
        {
            this.contextAccessor = contextAccessor;
        }

        /// <summary>
        /// The rootpath and pluginpath were already set, this should be /bin/debug/netcoreapp3.0/Plugins/PluginA
        /// Now, we need to load the correct assembly from this path, by suffixing it with PluginA.dll
        /// </summary>
        /// <returns>The name of the assembly to load. 'PluginA', for example</returns>

        public override string GetAssemblyName()
        {
            var pluginType = this.contextAccessor.HttpContext.Request.Headers["PluginType"].First();

            return $"{pluginType}.dll";
        }
    }
}