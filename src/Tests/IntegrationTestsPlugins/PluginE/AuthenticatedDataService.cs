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
        private readonly ITokenService tokenService;
        internal AuthenticatedDataService(ITokenService tokenService)
        {
            this.tokenService = tokenService;
        }

        [PluginFactory]
        public static AuthenticatedDataService Factory(IPluginServiceProvider pluginServiceProvider)
        {
            var tokenService = pluginServiceProvider.GetPluginService<ITokenService>();
            return new AuthenticatedDataService(tokenService);
        }

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
