using AppHost.ViewModels;
using Avalonia.Controls;

namespace AppHost
{
    public static class ViewModelLocator
    {
        public static Shared.ViewModelBase MainViewModel
        {
            get
            {
                return AppServiceLocator.GetService<MainWindowViewModel>();
            }
        }
    }
}