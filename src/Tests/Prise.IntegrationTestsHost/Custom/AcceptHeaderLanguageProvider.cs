using System.Threading.Tasks;
using ExternalServices;
using Microsoft.AspNetCore.Http;

namespace Prise.IntegrationTestsHost.Custom
{
    public class AcceptHeaderLanguageProvider : ICurrentLanguageProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public AcceptHeaderLanguageProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<CurrentLanguage> GetCurrentLanguage()
        {
            return new CurrentLanguage { LanguageCultureCode = this.httpContextAccessor.HttpContext.Request.Headers["Accept-Language"][0] };
        }
    }
}
