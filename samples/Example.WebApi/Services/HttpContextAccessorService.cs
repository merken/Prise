using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Example.Contract;
using System.IO;

namespace Example.WebApi.Services
{
    public class HttpContextAccessorService : IHttpContextAccessorService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextAccessorService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetHttpBody()
        {
            var body = this.httpContextAccessor.HttpContext.Request.Body;
            using (StreamReader stream = new StreamReader(body))
            {
                return await stream.ReadToEndAsync();
            }
        }
    }
}