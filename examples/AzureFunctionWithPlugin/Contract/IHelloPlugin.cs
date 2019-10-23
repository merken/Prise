using System;
using System.Threading.Tasks;

namespace Contract
{
    public interface IHelloPlugin
    {
        Task<string> SayHello(string input);
    }
}
