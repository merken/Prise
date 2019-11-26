using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace MyHost
{
    public class LanguageBasedAssemblyNameProvider<T> : IPluginAssemblyNameProvider<T>
    {
        private bool disposed = false;
        private readonly IHttpContextAccessor contextAccessor;
        public LanguageBasedAssemblyNameProvider(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public string GetAssemblyName()
        {
            var language = this.contextAccessor.HttpContext.Request.Headers["Accept-Language"].First();
            var assemblyName = "Hello.Plugin.dll";
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

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Nothing to do here
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}