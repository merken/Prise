using System;
using System.Runtime.Serialization;

namespace Prise.Proxy.Exceptions
{
    [Serializable]
    public class PriseProxyException : Exception
    {
        public PriseProxyException(string message) : base(message)
        {
        }

        public PriseProxyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PriseProxyException()
        {
        }

        protected PriseProxyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
