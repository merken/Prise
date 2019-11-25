using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using ProductsReaderPlugin;
using System;

namespace PluginTester
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", true, true)
              .Build();
            var bootstrapper = new TableStorageProductsReaderBootstrapper();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            var serviceProvider = bootstrapper.Bootstrap(services).BuildServiceProvider();
            var reader = TableStorageProductsReader.ThisIsTheFactoryMethod(serviceProvider);

            Console.WriteLine(reader.All().Result);
            Console.Read();
        }
    }
}
