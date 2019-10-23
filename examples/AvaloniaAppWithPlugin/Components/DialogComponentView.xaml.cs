using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Components
{
    public class DialogComponentView : UserControl
    {
        public DialogComponentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
