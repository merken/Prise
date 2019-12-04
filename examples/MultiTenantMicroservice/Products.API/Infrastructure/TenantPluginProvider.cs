using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace Products.API.Infrastructure
{
    public class TenantPluginProvider<T> :
        IPluginPathProvider<T>,
        IPluginAssemblyNameProvider<T>,
        IDependencyPathProvider<T>
    {
        protected readonly IHttpContextAccessor contextAccessor;
        protected readonly TenantConfig tenantConfig;

        public TenantPluginProvider(IHttpContextAccessor contextAccessor, TenantConfig tenantConfig)
        {
            this.tenantConfig = tenantConfig;
            this.contextAccessor = contextAccessor;
        }

        public string GetAssemblyName() => $"{GetPluginFromContext()}.dll";
        public string GetPluginPath() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", GetPluginFromContext());
        public string GetDependencyPath() => GetPluginPath();

        private string GetPluginFromContext()
        {
            if (!this.contextAccessor.HttpContext.Request.Headers["Tenant"].Any())
                return "SQLPlugin"; //The old plugin is a netcoreapp2.1 plugin, it should work on both MyHost and MyHost2

            var tenant = this.contextAccessor.HttpContext.Request.Headers["Tenant"].First();
            var configPair = this.tenantConfig.Configuration
                .FirstOrDefault(c => String.Compare(c.Tenant, tenant, StringComparison.OrdinalIgnoreCase) == 0);
            return configPair.Plugin;
        }

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