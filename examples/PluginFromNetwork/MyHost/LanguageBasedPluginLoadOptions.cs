using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Prise.Infrastructure;

namespace MyHost
{
    public class LanguageBasedPluginLoadOptions<T> : NetworkAssemblyLoaderOptions<T>
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IConfiguration configuration;
        public LanguageBasedPluginLoadOptions(IHttpContextAccessor contextAccessor, IConfiguration configuration)
            : base(String.Empty, // BaseUrl will be provided at runtime
                  ignorePlatformInconsistencies: true) // The plugins are netstandard, so ignore incosistencies with the MyHost (netcoreapp3.0)
        {
            this.configuration = configuration;
            this.contextAccessor = contextAccessor;
        }

        public override string BaseUrl
        {
            get
            {
                var language = this.contextAccessor.HttpContext.Request.Headers["Accept-Language"].First();
                var plugin = "Hello.Plugin";
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
                return $"{configuration["PluginServerUrl"]}/{plugin}";
            }
        }
    }
}