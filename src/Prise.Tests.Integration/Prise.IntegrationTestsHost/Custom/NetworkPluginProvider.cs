using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;
using Prise.IntegrationTestsContract;

namespace Prise.IntegrationTestsHost.Custom
{
    public class NetworkPluginProvider
        : DefaultNetworkAssemblyLoaderOptions<INetworkCalculationPlugin>,
            IPluginAssemblyNameProvider<INetworkCalculationPlugin>
    {
        private bool disposed = false;
        private readonly IHttpContextAccessor contextAccessor;
        public NetworkPluginProvider(IHttpContextAccessor contextAccessor)
            : base(String.Empty) // BaseUrl will be provided at runtime
        {
            this.contextAccessor = contextAccessor;
        }

        public override string BaseUrl
        {
            get
            {
                var pluginType = this.contextAccessor.HttpContext.Request.Headers["PluginType"].First();
                return $"https://localhost:5001/Plugins/{pluginType}";
            }
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
