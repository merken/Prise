using System.Linq;
using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;
using Prise.Infrastructure.NetCore.Contracts;

namespace MyHost.Infrastructure
{
    public class TenantPluginAssemblyLoadOptions : TenantAwarePluginMiddleware, ILocalAssemblyLoaderOptions
    {
        public TenantPluginAssemblyLoadOptions(IHttpContextAccessor contextAccessor, TenantConfig tenantConfig)
            : base(contextAccessor, tenantConfig) { }
        public string PluginPath => GetPlugin();
        public DependencyLoadPreference DependencyLoadPreference => DependencyLoadPreference.PreferDependencyContext;
    }
}