using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise;

namespace MyHost
{
    public class LanguageBasedAssemblyNameProvider<T> : PluginAssemblyNameProvider<T>
    {
        private readonly IHttpContextAccessor contextAccessor;
        public LanguageBasedAssemblyNameProvider(IHttpContextAccessor contextAccessor)
            : base(String.Empty) // AssemblyName will be provided at runtime
        {
            this.contextAccessor = contextAccessor;
        }

        public override string GetAssemblyName()
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