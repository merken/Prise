using Contract;
using Prise.Plugin;
using Avalonia.Controls;

namespace Components
{
    [Plugin(PluginType=typeof(IAppComponent))]
    public class HelloWorldComponent : IAppComponent
    {
        public UserControl Load()
        {
            return new HelloWorldComponentView();
        }

        public string GetName()
        {
            return nameof(HelloWorldComponent);
        }
    }
}