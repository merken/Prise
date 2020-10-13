using Example.Contract;
using Prise.Plugin;

namespace MvcPlugin.DataStorage
{
    [Plugin(PluginType = typeof(IMvcPlugin))]
    [MvcPluginDescriptionAttribute(Description = "This feature will add the 'api/azure' and 'api/sql' API Controllers to the current MVC Host. These controllers will retrieve data from Azure Table Storage and Sql.")]
    public class DataStorageMvcPlugin : IMvcPlugin
    {
        // Nothing to do here
    }
}