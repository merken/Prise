using System;

namespace Prise.Proxy
{
    public class PassthroughResultConverter : ResultConverter
    {
        public override object Deserialize(Type localType, Type remoteType, object value)
        {
            return value;
        }
    }
}