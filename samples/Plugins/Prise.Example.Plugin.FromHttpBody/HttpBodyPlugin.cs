using System;
using Prise.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prise.Example.Contract;
using System.Text.Json;

namespace Prise.Example.Plugin.FromHttpBody
{
    [Plugin(PluginType = typeof(IPlugin))]
    public class HttpBodyPlugin : IPlugin
    {
        [PluginService(ProvidedBy = ProvidedBy.Host, ServiceType = typeof(IHttpContextAccessorService), BridgeType = typeof(HttpContextAccessorService))]
        private readonly IHttpContextAccessorService httpContextAccessor;

        [PluginActivated]
        public void OnActivated()
        {
            // TODO some activation code here
        }

        public async Task<MyDto> Create(int number, string text)
        {
            throw new NotSupportedException($"Cannot write to HTTP Context");
        }

        public async Task<IEnumerable<MyDto>> GetAll()
        {
            var httpBody = await httpContextAccessor.GetHttpBody();
            var results = System.Text.Json.JsonSerializer.Deserialize<List<MyDto>>(httpBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            return results;
        }
    }
}
