using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Prise.Infrastructure.NetCore
{
    public class BinaryFormatterResultConverter : ResultConverter
    {
        public override object Deserialize(Type localType, Type remoteType, object value)
        {
            return DeserializeFromStream(SerializeToStream(value));
        }

        public static MemoryStream SerializeToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }

        public static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return o;
        }
    }
}