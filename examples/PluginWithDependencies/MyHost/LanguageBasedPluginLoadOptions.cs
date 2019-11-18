using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace MyHost
{
    public class LanguageBasedPluginLoadOptions : LocalAssemblyLoaderOptions
    {
        private readonly IHttpContextAccessor contextAccessor;
        public LanguageBasedPluginLoadOptions(IHttpContextAccessor contextAccessor)
            : base(String.Empty, // PluginPath will be provided at runtime
                  ignorePlatformInconsistencies: true) // The plugins are netstandard plugins, the host is netcoreapp, ignore this inconsistency
        {
            this.contextAccessor = contextAccessor;
        }

        public override string PluginPath
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