using Prise.Plugin;
using Example.Contract;

namespace TwitterWidgetPlugin
{
    [Plugin(PluginType = typeof(IMvcPlugin))]
    [MvcPluginDescription(Description = "This feature will add the '/twitter' widget to the current MVC Host.")]
    public class TwitterWidgetMVCPlugin : IMvcPlugin
    {
        // Nothing to do here, just some feature discovery happening...
    }
}

