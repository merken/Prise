using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost
{
    public static class AvaloniaServiceCollectionExtensions
    {
        public static AppBuilder ConfigureServices(this AppBuilder app, Action<IServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices.Invoke(services);

            AppServiceLocator.Configure(services.BuildServiceProvider());

            return app;
        }
    }
}