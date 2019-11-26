using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyHost
{
    public class LanguageBasedPluginPathProvider<T> : DefaultPluginPathProvider<T>
    {
        private readonly IHttpContextAccessor contextAccessor;
        public LanguageBasedPluginPathProvider(IHttpContextAccessor contextAccessor) : base(String.Empty)
        {
            this.contextAccessor = contextAccessor;
        }

        public override string GetPluginPath()
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

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", plugin);
        }

    }
}
