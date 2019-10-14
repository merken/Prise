using System;
using Newtonsoft.Json;

namespace Prise.Infrastructure.NetCore
{
    public class NewtonsoftParameterConverter : IParameterConverter
    {
        public object ConvertToRemoteType(Type localType, object value)
        {
            var json = JsonConvert.SerializeObject(value);
            return JsonConvert.DeserializeObject(json, localType);
        }
    }
}