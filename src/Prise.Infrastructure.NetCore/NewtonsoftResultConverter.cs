using System;
using Newtonsoft.Json;

namespace Prise.Infrastructure.NetCore
{
    public class NewtonsoftResultConverter : ResultConverterBase
    {
        public override object Deserialize(Type localType, Type remoteType, object value)
        {
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value), localType);
        }
    }
}