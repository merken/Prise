using System;

namespace Prise.Infrastructure.NetCore
{
    public class PassthroughResultConverter : ResultConverter
    {
        public override object Deserialize(Type localType, Type remoteType, object value)
        {
            return value;
        }
    }
}