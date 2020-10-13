namespace Prise.IntegrationTestsContract
{
    public class ObjectToSerialize
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
        public double DoubleProperty { get; set; }
    }

    public interface IPluginWithSerializer
    {
        string SerializeObject(ObjectToSerialize obj);
    }
}
