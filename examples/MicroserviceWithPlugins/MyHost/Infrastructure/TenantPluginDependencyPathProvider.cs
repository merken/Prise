using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace MyHost.Infrastructure
{
    public class TenantPluginDependencyPathProvider<T> : TenantAwarePluginMiddleware, IDependencyPathProvider<T>
    {
        private readonly IPluginPathProvider<T> pluginPathProvider;
        public TenantPluginDependencyPathProvider(
            IHttpContextAccessor contextAccessor,
            TenantConfig tenantConfig,
            IPluginPathProvider<T> pluginPathProvider
            )
            : base(contextAccessor, tenantConfig)
        {
            this.pluginPathProvider = pluginPathProvider;
        }

        public string GetDependencyPath() => Path.Combine(this.pluginPathProvider.GetPluginPath());

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}