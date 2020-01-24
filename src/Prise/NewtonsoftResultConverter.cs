#if NETCORE2_1
using System;
using Newtonsoft.Json;
using Prise.Proxy;

namespace Prise
{
    public class NewtonsoftResultConverter : ResultConverter
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

            return JsonConvert.DeserializeObject(
                    JsonConvert.SerializeObject(value), // First, serialize the object into a string
                    resultType); // Second, deserialize it using the correct type
        }
    }
}
#endif