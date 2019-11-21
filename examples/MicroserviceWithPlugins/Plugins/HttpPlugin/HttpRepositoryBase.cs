using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpPlugin
{
    public abstract class HttpRepositoryBase
    {
        private readonly HttpClient client;
        protected HttpRepositoryBase(HttpClient client, Uri baseUrl)
        {
            this.client = client;
            this.client.BaseAddress = baseUrl;
        }

        protected async Task<T> SendAync<T>(HttpMethod httpMethod, string endpoint, object payload = null)
        {
            var message = new HttpRequestMessage(httpMethod, endpoint);

            if (payload != null)
            {
                var json = JsonConvert.SerializeObject(payload);
                message.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await this.client.SendAsync(message);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}
