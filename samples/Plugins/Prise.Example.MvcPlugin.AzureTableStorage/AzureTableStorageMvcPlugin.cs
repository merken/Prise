using Prise.Example.Contract;
using Prise.Plugin;

namespace Prise.Example.MvcPlugin.AzureTableStorage
{
    [Plugin(PluginType = typeof(IMvcPlugin))]
    [MvcPluginDescriptionAttribute(Description = "This feature will add the 'api/azure' API Controller to the current MVC Host. This controller retrieves data from Azure Table Storage.")]
    public class AzureTableStorageMvcPlugin : IMvcPlugin
    {
        // Nothing to do here
    }
}