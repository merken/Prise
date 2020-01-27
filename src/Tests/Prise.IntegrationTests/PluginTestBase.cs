using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Prise.IntegrationTestsContract;

namespace Prise.IntegrationTests
{
    public abstract class PluginTestBase
    {
        protected readonly HttpClient _client;
        protected readonly AppHostWebApplicationFactory _factory;

        protected PluginTestBase(
                 AppHostWebApplicationFactory factory)
        {
            _factory = factory;
            var local = Environment.GetEnvironmentVariable("LOCAL") == "true";
            if (local)
            {
                _client = new HttpClient();
                _client.BaseAddress = new Uri("https://localhost:5001");
            }
            else
                _client = factory.CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    BaseAddress = new Uri("https://localhost:5001")
                });
        }
    }
}