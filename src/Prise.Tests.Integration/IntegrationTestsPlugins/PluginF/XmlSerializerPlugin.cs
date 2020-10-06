using Prise.IntegrationTestsContract;
using Prise.Plugin;
using XmlSerializerLib;

namespace PluginF
{
    [Plugin(PluginType = typeof(IPluginWithSerializer))]
    public class XmlSerializerPlugin : IPluginWithSerializer
    {
        public string SerializeObject(ObjectToSerialize obj)
        {
            return XmlSerializerUtil.SerializeToXml<ObjectToSerialize>(obj);
        }
    }
}
