using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;
using Prise;

namespace MyHost.Infrastructure
{
    public class TenantPluginNetworkLoadOptions<T> : TenantAwarePluginMiddleware, INetworkAssemblyLoaderOptions<T>
    {
        public TenantPluginNetworkLoadOptions(IHttpContextAccessor contextAccessor, TenantConfig tenantConfig)
           : base(contextAccessor, tenantConfig) { }

        public string BaseUrl => $"https://localhost:5003/Plugins/{GetPluginPathFromContext()}";
        public PluginPlatformVersion PluginPlatformVersion => PluginPlatformVersion.Empty();
        public bool IgnorePlatformInconsistencies => false;
        public NativeDependencyLoadPreference NativeDependencyLoadPreference => NativeDependencyLoadPreference.PreferInstalledRuntime;

    }
}
