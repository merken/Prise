using Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using ProductsDeleterPlugin;
using ProductsReaderPlugin;
using ProductsWriterPlugin;
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

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);

            var reader = GetReader(services);
            var writer = GetWriter(services);
            var deleter = GetDeleter(services);

            writer.Create(new Product
            {
                Id = 12,
                Description = "TEST",
                Name = "TESTE",
                SKU = "TESKU"
            }).Wait();

            var results = reader.All().Result;
            foreach (var r in results)
                Console.WriteLine(r.Name);

            deleter.Delete(12).Wait();

            results = reader.All().Result;
            foreach (var r in results)
                Console.WriteLine(r.Name);

            Console.Read();
        }

        private static IProductsReader GetReader(IServiceCollection services) =>
            TableStorageProductsReader.ThisIsTheFactoryMethod(new TableStorageProductsReaderBootstrapper().Bootstrap(services).BuildServiceProvider());
        private static IProductsWriter GetWriter(IServiceCollection services) =>
            TableStorageProductsWriter.ThisIsTheFactoryMethod(new TableStorageProductsWriterBootstrapper().Bootstrap(services).BuildServiceProvider());
        private static IProductsDeleter GetDeleter(IServiceCollection services) =>
            TableStorageProductsDeleter.ThisIsTheFactoryMethod(new TableStorageProductsDeleterBootstrapper().Bootstrap(services).BuildServiceProvider());

    }
}
