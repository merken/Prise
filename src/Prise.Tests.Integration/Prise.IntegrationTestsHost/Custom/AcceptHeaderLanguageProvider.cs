using System.Threading.Tasks;
using ExternalServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Prise.IntegrationTestsHost.Custom
{
    // This only lives in the Host
    public interface IContextLanguageProvider
    {
        Task<string> GetCurrentLanguage();
    }

    // This only lives in the Host
    public class HttpContextLanguageProvider : IContextLanguageProvider
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        public HttpContextLanguageProvider(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
        }
        public Task<string> GetCurrentLanguage() => Task.FromResult(this.httpContextAccessor.HttpContext.Request.Headers["Accept-Language"][0]);
    }

    // This interface lives inside the contract
    // The implementation instance lives in the Host
    public class AcceptHeaderLanguageProvider2 : ICurrentLanguageProvider
    {
        private readonly IContextLanguageProvider contextLanguageProvider;
        public AcceptHeaderLanguageProvider2(IContextLanguageProvider contextLanguageProvider)
        {
            this.contextLanguageProvider = contextLanguageProvider;
        }

        public async Task<CurrentLanguage> GetCurrentLanguage()
        {
            return new CurrentLanguage { LanguageCultureCode = await this.contextLanguageProvider.GetCurrentLanguage() };
        }
    }

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
