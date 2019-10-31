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

        protected string GetPlugin()
        {
            if (!this.contextAccessor.HttpContext.Request.Headers["Tenant"].Any())
                throw new NotSupportedException("Please provide Tenant HTTP Header");
            var tenant = this.contextAccessor.HttpContext.Request.Headers["Tenant"].First();
            var configPair = this.tenantConfig.Configuration
                .FirstOrDefault(c => String.Compare(c.Tenant, tenant, StringComparison.OrdinalIgnoreCase) == 0);
            return configPair.Plugin;
        }
    }
}