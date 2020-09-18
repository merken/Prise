using System;
using System.Runtime.Serialization;

namespace Prise.Platform
{
    [Serializable]
    public class PlatformException : Exception
    {
        public PlatformException(string message) : base(message)
        {
        }

        public PlatformException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PlatformException()
        {
        }

        protected PlatformException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}