using System;

namespace Prise
{
    public class PassthroughResultConverter : ResultConverter
    {
        public override object Deserialize(Type localType, Type remoteType, object value)
        {
            return value;
        }
    }
}