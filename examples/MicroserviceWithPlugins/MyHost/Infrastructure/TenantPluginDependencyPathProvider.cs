using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace MyHost.Infrastructure
{
    public class TenantPluginDependencyPathProvider : TenantAwarePluginMiddleware, IDependencyPathProvider
    {
        private readonly IRootPathProvider rootPathProvider;
        public TenantPluginDependencyPathProvider(
            IHttpContextAccessor contextAccessor,
            TenantConfig tenantConfig,
            IRootPathProvider rootPathProvider
            )
            : base(contextAccessor, tenantConfig)
        {
            this.rootPathProvider = rootPathProvider;
        }

        public string GetDependencyPath() => Path.Combine(this.rootPathProvider.GetRootPath(), $"{GetPluginPathFromContext()}");

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