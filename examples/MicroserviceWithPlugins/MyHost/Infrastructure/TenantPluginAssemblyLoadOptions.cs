using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;
using Prise.Infrastructure.NetCore;

namespace MyHost.Infrastructure
{
    public class TenantPluginAssemblyLoadOptions : TenantAwarePluginMiddleware, ILocalAssemblyLoaderOptions
    {
        public TenantPluginAssemblyLoadOptions(IHttpContextAccessor contextAccessor, TenantConfig tenantConfig)
            : base(contextAccessor, tenantConfig) { }
        public string PluginPath => GetPluginPath();
        public bool IgnorePlatformInconsistencies => false;
        public PluginPlatformVersion PluginPlatformVersion => PluginPlatformVersion.Empty(); // Allow for runtime platform scanning
        public DependencyLoadPreference DependencyLoadPreference => DependencyLoadPreference.PreferDependencyContext;
        public NativeDependencyLoadPreference NativeDependencyLoadPreference => NativeDependencyLoadPreference.PreferInstalledRuntime;
    }
}