using Contract;
using Prise.Plugin;
using Avalonia.Controls;

namespace Components
{
    [Plugin(PluginType=typeof(IAppComponent))]
    public class BonjourToutLeMondeComponent : IAppComponent
    {
        public UserControl Load()
        {
            return new BonjourToutLeMondeComponentView();
        }

        public string GetName()
        {
            return nameof(BonjourToutLeMondeComponent);
        }
    }
}