using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace MyHost.Infrastructure
{
    /// <summary>
    /// Base class that handles the reading out the Tenant HTTP Header value from the HTTP Context
    /// </summary>
    public abstract class TenantAwarePluginMiddleware
    {
        protected readonly IHttpContextAccessor contextAccessor;
        protected readonly TenantConfig tenantConfig;

        public TenantAwarePluginMiddleware(IHttpContextAccessor contextAccessor, TenantConfig tenantConfig)
        {
            this.tenantConfig = tenantConfig;
            this.contextAccessor = contextAccessor;
        }

        protected string GetPluginPathFromContext()
        {
            if (!this.contextAccessor.HttpContext.Request.Headers["Tenant"].Any())
                return "OldSQLPlugin"; //The old plugin is a netcoreapp2.1 plugin, it should work on both MyHost and MyHost2

            var tenant = this.contextAccessor.HttpContext.Request.Headers["Tenant"].First();
            var configPair = this.tenantConfig.Configuration
                .FirstOrDefault(c => String.Compare(c.Tenant, tenant, StringComparison.OrdinalIgnoreCase) == 0);
            return configPair.Plugin;
        }

        protected string GetPluginAssemblyFromContext()
        {
            if (!this.contextAccessor.HttpContext.Request.Headers["Tenant"].Any())
                return "OldSQLPlugin";

            var tenant = this.contextAccessor.HttpContext.Request.Headers["Tenant"].First();
            var configPair = this.tenantConfig.Configuration
                .FirstOrDefault(c => String.Compare(c.Tenant, tenant, StringComparison.OrdinalIgnoreCase) == 0);
            return configPair.Plugin;
        }
    }
}