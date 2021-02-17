using Avalonia;
using AppHost.Views;
using Microsoft.Extensions.DependencyInjection;
using AppHost.ViewModels;
using Contract;
using System;
using Prise.DependencyInjection;
using AppHost.Infrastructure;
using System.IO;
using Avalonia.ReactiveUI;
using Prise.Proxy;

namespace AppHost
{
    public class Program
    {
        public static void Main(string[] args) => BuildAvaloniaApp().Start<MainWindow>();

        // This method is needed for IDE previewer infrastructure
        static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                           .UsePlatformDetect()
                           .UseReactiveUI()
                           .ConfigureServices(ConfigureServices)
                           .LogToDebug();
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<MainWindowViewModel>()
                .AddPrise()
                .AddFactory<IResultConverter>(()=> new AvaloniaPluginResultConverter()
            );
        }
    }
}
