using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;
using Prise.Infrastructure.NetCore.Contracts;

namespace PluginServer.Custom
{
    public class ContextPluginAssemblyLoadOptions : ILocalAssemblyLoaderOptions
    {
        private readonly IHttpContextAccessor contextAccessor;
        public ContextPluginAssemblyLoadOptions(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }
        
        public string PluginPath
        {
            get
            {
                var pluginType = this.contextAccessor.HttpContext.Request.Headers["PluginType"].First();
                return pluginType;
            }
        }

        public DependencyLoadPreference DependencyLoadPreference => DependencyLoadPreference.PreferDependencyContext;
    }
}