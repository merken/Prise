using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using Prise.IntegrationTestsHost;

namespace Prise.IntegrationTests
{
    public class CommandLineArgumentsLazy : ICommandLineArguments
    {
        public bool UseLazyService => true;
    }

    public partial class AppHostWebApplicationFactory
       : WebApplicationFactory<Prise.IntegrationTestsHost.Startup>
    {
        private bool useLazyServices = false;
        private Dictionary<string, string> settings;

        public AppHostWebApplicationFactory ConfigureLazyService()
        {
            this.useLazyServices = true;
            return this;
        }

        public AppHostWebApplicationFactory AddInMemoryConfig(Dictionary<string, string> settings)
        {
            this.settings = settings;
            return this;
        }
    }
}