using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

        public async Task<IEnumerable<Data>> GetData(string token)
        {
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
