using System;
using System.Threading.Tasks;

namespace Contract
{
    public interface IHelloWorldPlugin
    {
        Task<string> SayHelloAsync(string language, string input);
        Task<HelloDictionary> GetHelloDictionaryAsync();
    }
}
