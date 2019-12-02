using System.Collections.Generic;

namespace Products.API.Infrastructure
{
    public class TenantConfig
    {
        public List<TenantConfigPair> Configuration { get; set; }
    }
    
    public class TenantConfigPair
    {
        public string Tenant { get; set; }
        public string Plugin { get; set; }
    }
}