using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;

namespace MyHost.Infrastructure
{
    public class TenantPluginAssemblyLoadOptions : TenantAwarePluginMiddleware, ILocalAssemblyLoaderOptions
    {
        public TenantPluginAssemblyLoadOptions(IHttpContextAccessor contextAccessor, TenantConfig tenantConfig)
            : base(contextAccessor, tenantConfig) { }
        public string PluginPath => GetPluginPathFromContext();
        public PluginPlatformVersion PluginPlatformVersion => PluginPlatformVersion.Empty(); // Allow for runtime platform scanning
        public bool IgnorePlatformInconsistencies => false;
        public DependencyLoadPreference DependencyLoadPreference => DependencyLoadPreference.PreferDependencyContext;
        public NativeDependencyLoadPreference NativeDependencyLoadPreference => NativeDependencyLoadPreference.PreferInstalledRuntime;
    }
}