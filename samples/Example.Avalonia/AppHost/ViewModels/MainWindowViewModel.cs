using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Contract;
using Prise;
using Shared;

namespace AppHost.ViewModels
{
    public class MainWindowViewModel : Shared.ViewModelBase
    {
        public RelayCommand<object> LoadAllComponentsCommand { get; set; }
        public RelayCommand<object> UnLoadAllComponentsCommand { get; set; }
        public RelayCommand<object> LoadComponentCommand { get; set; }
        public List<string> Components { get; set; }
        public UserControl CurrentControl { get; set; }

        private Dictionary<string, IAppComponent> components = new Dictionary<string, IAppComponent>();

        public MainWindowViewModel()
        {
            LoadAllComponentsCommand = new RelayCommand<object>(LoadComponents, true, true);
            UnLoadAllComponentsCommand = new RelayCommand<object>(UnLoadComponents, true, true);
            LoadComponentCommand = new RelayCommand<object>(LoadComponent, true, false);
        }

        async void LoadComponents(object parameter)
        {
            var pluginLoader = AppServiceLocator.GetService<IPluginLoader>();
            var dist = GetPathToComponentsPublishDirectory();
            var pluginScanResults = await pluginLoader.FindPlugins<IAppComponent>(dist);

            components = new Dictionary<string, IAppComponent>();
            foreach (var pluginScanResult in pluginScanResults)
            {
                var plugin = await pluginLoader.LoadPlugin<IAppComponent>(pluginScanResult, configure: (ctx) =>
                {
                    ctx
                        .AddHostTypes(new[] {typeof(Application)})
                    ;
                });
                
                components.Add(plugin.GetName(), plugin);
            }
            Components = new List<string>(components.Select(p => p.Key));
            this.RaisePropertyChanged(nameof(Components));
        }

        void UnLoadComponents(object parameter)
        {
            var pluginLoader = AppServiceLocator.GetService<IPluginLoader>(); 
            pluginLoader.UnloadAll();
            CurrentControl = null;
            Components = new List<string>();
            this.RaisePropertyChanged(nameof(CurrentControl));
            this.RaisePropertyChanged(nameof(Components));
        }

        void LoadComponent(object parameter)
        {
            var plugin = this.components[parameter.ToString()];
            CurrentControl = plugin.Load();
            this.RaisePropertyChanged(nameof(CurrentControl));
        }
        
        static string GetPathToComponentsPublishDirectory()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Components/bin/Debug/netcoreapp3.1"));
        }
    }
}