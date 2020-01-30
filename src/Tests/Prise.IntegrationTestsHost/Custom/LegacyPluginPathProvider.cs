using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Prise.IntegrationTestsHost.Custom
{
    public class LegacyPluginPathProvider : DefaultPluginPathProvider<Legacy.Domain.ITranslationPlugin>
    {
        private readonly IHttpContextAccessor contextAccessor;

        public LegacyPluginPathProvider(IHttpContextAccessor contextAccessor) : base(String.Empty)
        {
            if (contextAccessor == null)
                throw new ArgumentNullException(nameof(contextAccessor));

            this.contextAccessor = contextAccessor;
        }

        public override string GetPluginPath()
        {
            var preferredPluginVersion = this.contextAccessor.HttpContext.Request.Query.ContainsKey("version") ?
                this.contextAccessor.HttpContext.Request.Query["version"][0] :
                null;
            if (String.IsNullOrEmpty(preferredPluginVersion))
                throw new NotSupportedException($"Please provide a version in the querystring to use {typeof(Legacy.Domain.ITranslationPlugin).Name}");

            var path = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "LegacyPlugin")}{preferredPluginVersion}";
            return path;
        }
    }
}
