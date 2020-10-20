using Example.Contract;
using System.Threading.Tasks;
using Prise.Proxy;

namespace Plugin.FromHttpBody
{
    public class HttpContextAccessorService : ReverseProxy, IHttpContextAccessorService
    {
        public HttpContextAccessorService(object hostService) : base(hostService) { }

        public Task<string> GetHttpBody()
        {
            return this.InvokeOnHostService<Task<string>>();
        }
    }
}