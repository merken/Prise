using Example.Contract;
using System.Threading.Tasks;

namespace Plugin.FromHttpBody
{
    public class HttpContextAccessorService : Prise.PluginBridge.PluginBridge, IHttpContextAccessorService
    {
        public HttpContextAccessorService(object hostService) : base(hostService) { }

        public Task<string> GetHttpBody()
        {
            return this.InvokeThisMethodOnHostService<Task<string>>();
        }
    }
}