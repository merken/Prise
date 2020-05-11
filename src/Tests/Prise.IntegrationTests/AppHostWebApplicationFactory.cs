using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using Prise.IntegrationTestsHost;

namespace Prise.IntegrationTests
{
    public class CommandLineArgumentsLazy : ICommandLineArguments
    {
        public bool UseLazyService { get; set; }
        public bool UseCollectibleAssemblies { get; set; }
    }

    public partial class AppHostWebApplicationFactory
       : WebApplicationFactory<Prise.IntegrationTestsHost.Startup>
    {
        internal static AppHostWebApplicationFactory _instance = new AppHostWebApplicationFactory(new CommandLineArgumentsLazy(), null);
        internal static AppHostWebApplicationFactory Default() => _instance;

        private readonly Dictionary<string, string> settings;
        private readonly ICommandLineArguments commandLineArguments;
        public AppHostWebApplicationFactory(ICommandLineArguments commandLineArguments, Dictionary<string, string> settings = null)
        {
            this.commandLineArguments = commandLineArguments;
            this.settings = settings;
        }
    }
}