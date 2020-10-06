using System.Threading.Tasks;

namespace Prise.Tests
{
    public class MyServiceProxy : Prise.Proxy.ReverseProxy, IMyService
    {
        public MyServiceProxy(object originalObject) : base(originalObject) { }

        public Task<string> GetString()
        {
            return this.InvokeThisMethodOnHostService<Task<string>>();
        }
    }
}
