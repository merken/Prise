using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;
using Prise.Infrastructure.NetCore;

namespace MyHost.Infrastructure
{
    public class TenantPluginNetworkLoadOptions : TenantAwarePluginMiddleware, INetworkAssemblyLoaderOptions
    {
        public TenantPluginNetworkLoadOptions(IHttpContextAccessor contextAccessor, TenantConfig tenantConfig)
           : base(contextAccessor, tenantConfig) { }
        public string BaseUrl => $"https://localhost:5003/Plugins/{GetPluginPath()}";

        public PluginPlatformVersion PluginPlatformVersion => PluginPlatformVersion.Empty();
        public bool IgnorePlatformInconsistencies => false;
        public DependencyLoadPreference DependencyLoadPreference => DependencyLoadPreference.PreferDependencyContext;
        public NativeDependencyLoadPreference NativeDependencyLoadPreference => NativeDependencyLoadPreference.PreferInstalledRuntime;
    }
}
