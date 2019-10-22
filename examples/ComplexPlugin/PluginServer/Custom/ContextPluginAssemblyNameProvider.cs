using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace PluginServer.Custom
{
    public class ContextPluginAssemblyNameProvider : IPluginAssemblyNameProvider
    {
        private bool disposed = false;
        private readonly IHttpContextAccessor contextAccessor;
        public ContextPluginAssemblyNameProvider(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public string GetAssemblyName()
        {
            var pluginType = this.contextAccessor.HttpContext.Request.Headers["PluginType"].First();

            return $"{pluginType}.dll";
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