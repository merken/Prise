using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Prise.Infrastructure.NetCore.Contracts;

namespace MyHost
{
    public class LanguageBasedPluginLoadOptions : ILocalAssemblyLoaderOptions
    {
        private readonly IHttpContextAccessor contextAccessor;
        public LanguageBasedPluginLoadOptions(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public string PluginPath
        {
            get
            {
                var language = this.contextAccessor.HttpContext.Request.Headers["Accept-Language"].First();
                var plugin = "Random.Plugin";
                switch (language.ToLower().Split(',')[0])
                {
                    case "en-gb":
                    case "en-us":
                        plugin = "Hello.Plugin";
                        break;
                    case "nl-be":
                    case "nl-nl":
                        plugin = "Hallo.Plugin";
                        break;

                    case "fr-be":
                    case "fr-fr":
                        plugin = "Bonjour.Plugin";
                        break;
                }

                return $"Plugins/{plugin}";
            }
        }
    }
}