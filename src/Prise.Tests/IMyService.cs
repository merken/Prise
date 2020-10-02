using System.Threading.Tasks;

namespace Prise.Tests
{
    public interface IMyService
    {
        Task<string> GetString();
    }
}
