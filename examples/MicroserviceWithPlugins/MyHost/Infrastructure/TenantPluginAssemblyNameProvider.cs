using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace MyHost.Infrastructure
{
    public class TenantPluginAssemblyNameProvider<T> : TenantAwarePluginMiddleware, IPluginAssemblyNameProvider<T>
    {
        public TenantPluginAssemblyNameProvider(IHttpContextAccessor contextAccessor, TenantConfig tenantConfig)
            : base(contextAccessor, tenantConfig) { }

        public string GetAssemblyName() => $"{GetPluginAssemblyFromContext()}.dll";

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