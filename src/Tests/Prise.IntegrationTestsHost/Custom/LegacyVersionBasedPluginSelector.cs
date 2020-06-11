using System;
using System.Collections.Generic;
using System.Linq;
using Legacy.Domain;
using Microsoft.AspNetCore.Http;
using Prise.AssemblyScanning;
using Prise.Infrastructure;

namespace Prise.IntegrationTestsHost.Custom
{
    public class LegacyVersionBasedPluginSelector : IAssemblySelector<Legacy.Domain.ITranslationPlugin>
    {
        private readonly IHttpContextAccessor contextAccessor;
        public LegacyVersionBasedPluginSelector(IHttpContextAccessor contextAccessor)
        {
            if (contextAccessor == null)
                throw new ArgumentNullException(nameof(contextAccessor));

            this.contextAccessor = contextAccessor;
        }

        public IEnumerable<AssemblyScanResult<ITranslationPlugin>> SelectAssemblies(IEnumerable<AssemblyScanResult<ITranslationPlugin>> scanResults)
        {
            var preferredPluginVersion = this.contextAccessor.HttpContext.Request.Query.ContainsKey("version") ?
               this.contextAccessor.HttpContext.Request.Query["version"][0] :
               null;

            if (String.IsNullOrEmpty(preferredPluginVersion))
                return scanResults;

            return scanResults.Where(a=>a.AssemblyPath.Contains(preferredPluginVersion));
        }
    }
}
