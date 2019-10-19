using System;
using Contract;
using Microsoft.AspNetCore.Http;

namespace MyHost
{
    public class AcceptHeaderlanguageService : ISharedLanguageService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public AcceptHeaderlanguageService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        public string GetLanguage()
        {
            return this.httpContextAccessor.HttpContext.Request.Headers["Accept-Language"][0];
        }
    }
}
