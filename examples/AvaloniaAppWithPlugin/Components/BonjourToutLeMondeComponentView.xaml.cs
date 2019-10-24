using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Components
{
    public class BonjourToutLeMondeComponentView : UserControl
    {
        public BonjourToutLeMondeComponentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
