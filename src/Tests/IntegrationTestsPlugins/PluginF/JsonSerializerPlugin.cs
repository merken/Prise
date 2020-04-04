using Prise.IntegrationTestsContract;
using Prise.Plugin;

namespace PluginF
{
    [Plugin(PluginType = typeof(IPluginWithSerializer))]
    public class JsonSerializerPlugin : IPluginWithSerializer
    {
        public string SerializeObject(ObjectToSerialize obj)
        {
#if NETCORE3_0 || NETCORE3_1
            return System.Text.Json.JsonSerializer.Serialize(obj);
#endif
#if NETCORE2_1
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
#endif
        }
    }
}
