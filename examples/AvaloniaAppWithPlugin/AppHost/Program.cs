using Avalonia;
using Avalonia.Logging.Serilog;
using AppHost.Views;
using Microsoft.Extensions.DependencyInjection;
using AppHost.ViewModels;
using System.Threading;
using Contract;
using Prise.Infrastructure.NetCore;
using System;
using System.IO;
using Prise.Infrastructure;
using AppHost.Infrastructure;

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
                   .WithPluginAssemblyName("Components.dll")
                   .WithLocalDiskAssemblyLoader("Plugins", DependencyLoadPreference.PreferAppDomain)
                   .WithResultConverter<AvaloniaPluginResultConverter>()
                   .WithRootPath(GetRootPath())
           );
        }

        private static string GetRootPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
