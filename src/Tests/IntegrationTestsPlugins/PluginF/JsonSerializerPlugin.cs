using Prise.IntegrationTestsContract;
using Prise.Plugin;

namespace PluginF
{
    [Plugin(PluginType = typeof(IPluginWithSerializer))]
    public class JsonSerializerPlugin : IPluginWithSerializer
    {
        public string SerializeObject(ObjectToSerialize obj)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }
    }
}
