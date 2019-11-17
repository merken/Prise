using System.Collections.Generic;

namespace MyHost.Infrastructure
{
    public class TenantConfig
    {
        public List<TenantConfigPair> Configuration { get; set; }
    }
    
    public class TenantConfigPair
    {
        public string Tenant { get; set; }
        public string Plugin { get; set; }
        public string PluginDirectory { get; set; }
    }
}