using System.Threading.Tasks;

namespace Prise.Example.Contract
{
    public interface IHttpContextAccessorService
    {
        Task<string> GetHttpBody();
    }
}