using Prise.Console.Contract;

namespace Prise.Web
{
    public class AppSettingsConfigurationService : IConfigurationService
    {
        public string GetConfigurationValueForKey(string key)
        {
            return "Server=localhost,1433;Initial Catalog=TestData;Integrated Security=False;User Id=sa;Password=MyPass@word;";
        }
    }
}