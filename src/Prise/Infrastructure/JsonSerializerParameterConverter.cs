using Prise.Proxy;
using System;
using System.Text.Json;

namespace Prise.Infrastructure
{
    public class JsonSerializerParameterConverter : IParameterConverter
    {
        public object ConvertToRemoteType(Type localType, object value)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize(json, localType);
        }

        public void Dispose()
        {
        }
    }
}