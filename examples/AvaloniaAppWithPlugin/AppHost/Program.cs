using Avalonia;
using Avalonia.Logging.Serilog;
using AppHost.Views;
using Microsoft.Extensions.DependencyInjection;
using AppHost.ViewModels;
using Contract;
using Prise;
using System;
using Prise.Infrastructure;
using AppHost.Infrastructure;
using System.IO;

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
            services.AddSingleton<MainWindowViewModel>();
            services.AddPrise<IAppComponent>(options =>
               options
                    .WithPluginPath(GetRootPath())
                    .WithPluginAssemblyName("Components.dll")
                    .WithResultConverter<AvaloniaPluginResultConverter>()
           );
        }

        private static string GetRootPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        }
    }
}
