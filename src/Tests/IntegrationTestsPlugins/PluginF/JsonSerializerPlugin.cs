using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Prise.IntegrationTestsContract;
using Prise.Plugin;

namespace PluginF
{
    public class DoubleConverter : JsonConverter<double>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(double) == typeToConvert;
        }

        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("0.##"));
        }
    }

    [Plugin(PluginType = typeof(IPluginWithSerializer))]
    public class JsonSerializerPlugin : IPluginWithSerializer
    {
        public string SerializeObject(ObjectToSerialize obj)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DoubleConverter());
            return System.Text.Json.JsonSerializer.Serialize(obj, options);
        }
    }
}
