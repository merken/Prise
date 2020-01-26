using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prise.IntegrationTestsContract;
using Prise.Plugin;

namespace PluginE
{
    [Plugin(PluginType = typeof(ITokenService))]
    public class TokenService : ITokenService
    {
        // No CTOR required for this plugin, default will be invoked

        public async Task<string> GenerateToken()
        {
            var tokens = await this.GetTokensFromDisk();
            var random = new Random();
            var randomIndex = random.Next(0, tokens.Count() - 1);
            return tokens.ElementAt(randomIndex);
        }

        public async Task<bool> ValidateToken(string token)
        {
            var tokens = await this.GetTokensFromDisk();
            if (tokens.Any(t => t == token))
                return true;
            return false;
        }

        private async Task<IEnumerable<String>> GetTokensFromDisk()
        {
            var filePath = Path.Combine(GetLocalExecutionPath(), $"tokens.json");
            if (!File.Exists(filePath))
                return null;

            using (var stream = new StreamReader(filePath))
            {
                var json = await stream.ReadToEndAsync();
                return JsonConvert.DeserializeObject<IEnumerable<string>>(json);
            }
        }

        private string GetLocalExecutionPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
