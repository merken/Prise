using System.Threading.Tasks;

namespace Prise.IntegrationTestsContract
{
    public interface ITokenService
    {
        Task<string> GenerateToken();

        Task<bool> ValidateToken(string token);
    }
}
