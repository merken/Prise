using Prise.Example.Contract;
using Prise.Plugin;

namespace Prise.Example.MvcPlugin.Sql
{
    [Plugin(PluginType = typeof(IMVCPlugin))]
    [MVCFeatureDescription(Description = "This feature will add the 'api/sql' API Controller to the current MVC Host. This API gets data from SQL Server via EF Core.")]
    public class SqlMVCPlugin : IMVCPlugin
    {
        // nothing to do here, this is just a placeholder for discovery
    }
}
