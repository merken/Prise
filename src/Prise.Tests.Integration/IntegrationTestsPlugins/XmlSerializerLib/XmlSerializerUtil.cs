using System;
using System.IO;
using System.Xml.Serialization;

namespace XmlSerializerLib
{
    public static class XmlSerializerUtil
    {
        // TFROM Lives outside of this assembly, XMLSerializer tries to create a dynamic assembly from TFROM and this fails.
        // This why the assembly must be marked as non-collectable
        public static string SerializeToXml<TFROM>(Object obj)
        {
            var xmlSerializer = new XmlSerializer(((TFROM)obj).GetType());
            using (StringWriter textWriter = new StringWriter())
            {

                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                xmlSerializer.Serialize(textWriter, obj, namespaces);
                return textWriter.ToString();
            }
        }
    }
}
