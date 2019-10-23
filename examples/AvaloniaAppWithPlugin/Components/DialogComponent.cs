using Contract;
using Prise.Infrastructure;
using Avalonia.Controls;

namespace Components
{
    [Plugin(PluginType=typeof(IAppComponent))]
    public class DialogComponent : IAppComponent
    {
        public UserControl Load()
        {
            return new DialogComponentView();
        }

        public string GetName()
        {
            return nameof(DialogComponent);
        }

        // TODO Shared services
    }
}