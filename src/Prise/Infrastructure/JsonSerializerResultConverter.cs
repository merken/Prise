using System;
using System.Text.Json;
using Prise.Proxy;

namespace Prise.Infrastructure
{
    public class JsonSerializerResultConverter : ResultConverter
    {
        public override object Deserialize(Type localType, Type remoteType, object value)
        {
            // Get the local type
            var resultType = localType;
            // Check if the type is a Task<T>
            if (localType.BaseType == typeof(System.Threading.Tasks.Task))
            {
                // Get the <T>
                resultType = localType.GenericTypeArguments[0];
            }

            return JsonSerializer.Deserialize(
                    JsonSerializer.Serialize(value), // First, serialize the object into a string
                    resultType); // Second, deserialize it using the correct type
        }
    }
}