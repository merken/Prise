using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Components
{
    public class HalloWereldComponentView : UserControl
    {
        public HalloWereldComponentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
