using Microsoft.AspNetCore.Http;
using MyHost.Infrastructure;
using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyHost.Infrastructure
{
    public class TenantPluginPathProvider<T> : TenantAwarePluginMiddleware, IPluginPathProvider<T>
    {
        public TenantPluginPathProvider(IHttpContextAccessor contextAccessor, TenantConfig tenantConfig)
           : base(contextAccessor, tenantConfig) { }

        public string GetPluginPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", GetPluginPathFromContext());
        }

        public void Dispose()
        {
            // nothing to do here
        }
    }
}
