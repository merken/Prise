using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Component
{
    public class ComponentView : UserControl
    {
        public ComponentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
