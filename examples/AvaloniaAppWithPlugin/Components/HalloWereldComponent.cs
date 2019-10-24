using Contract;
using Prise.Infrastructure;
using Avalonia.Controls;

namespace Components
{
    [Plugin(PluginType=typeof(IAppComponent))]
    public class HalloWereldComponent : IAppComponent
    {
        public UserControl Load()
        {
            return new HalloWereldComponentView();
        }

        public string GetName()
        {
            return nameof(HalloWereldComponent);
        }
    }
}