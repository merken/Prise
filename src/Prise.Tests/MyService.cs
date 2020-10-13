using System.Threading.Tasks;

namespace Prise.Tests
{
    public class MyService : IMyService
    {
        public Task<string> GetString()
        {
            return Task.FromResult("Test");
        }
    }
}
