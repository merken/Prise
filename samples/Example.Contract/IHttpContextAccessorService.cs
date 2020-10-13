using System.Threading.Tasks;

namespace Example.Contract
{
    public interface IHttpContextAccessorService
    {
        Task<string> GetHttpBody();
    }
}