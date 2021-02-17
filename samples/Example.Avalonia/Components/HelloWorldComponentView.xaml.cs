using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Components
{
    public class HelloWorldComponentView : UserControl
    {
        public HelloWorldComponentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
