using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace MyHost
{
    public class LanguageBasedAssemblyNameProvider : IPluginAssemblyNameProvider
    {
        private readonly IHttpContextAccessor contextAccessor;
        public LanguageBasedAssemblyNameProvider(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public string GetAssemblyName()
        {
            var language = this.contextAccessor.HttpContext.Request.Headers["Accept-Language"].First();
            var assemblyName = "Random.Plugin.dll";
            switch (language.ToLower())
            {
                case "en-gb":
                case "en-us":
                    assemblyName = "Hello.Plugin.dll";
                    break;
                case "nl-be":
                case "nl-nl":
                    assemblyName = "Hallo.Plugin.dll";
                    break;

                case "fr-be":
                case "fr-fr":
                    assemblyName = "Bonjour.Plugin.dll";
                    break;
            }
            return assemblyName;
        }
    }
}