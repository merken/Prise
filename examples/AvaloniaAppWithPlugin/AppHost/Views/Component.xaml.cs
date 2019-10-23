using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AppHost.Views
{
    public class Component : UserControl
    {
        public Component()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}