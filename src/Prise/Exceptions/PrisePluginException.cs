using System;
using System.Runtime.Serialization;

namespace Prise
{
    [Serializable]
    public class PrisePluginException : Exception
    {
        public PrisePluginException(string message) : base(message)
        {
        }

        public PrisePluginException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PrisePluginException()
        {
        }

        protected PrisePluginException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
