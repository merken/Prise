using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prise.IntegrationTestsContract;
using Prise.Plugin;

namespace PluginE
{
    [Plugin(PluginType = typeof(IAuthenticatedDataService))]
    public class AuthenticatedDataService : IAuthenticatedDataService
    {
        [PluginService(ServiceType =typeof(ITokenService))]
        private readonly ITokenService tokenService;

        private string wasPluginActivated = String.Empty;

        [PluginActivated]
        public void OnActivated()
        {
            this.wasPluginActivated = "ACTIVATED!";
        }

        public async Task<IEnumerable<Data>> GetData(string token)
        {
            if (String.IsNullOrEmpty(this.wasPluginActivated))
                throw new ArgumentException($"Plugin was not activated!");

            await this.tokenService.ValidateToken(token);
            return await this.GetDataFromDisk();
        }

        private async Task<IEnumerable<Data>> GetDataFromDisk()
        {
            var filePath = Path.Combine(GetLocalExecutionPath(), $"data.json");
            if (!File.Exists(filePath))
                return null;

            using (var stream = new StreamReader(filePath))
            {
                var json = await stream.ReadToEndAsync();
                return JsonConvert.DeserializeObject<IEnumerable<Data>>(json);
            }
        }

        private string GetLocalExecutionPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
