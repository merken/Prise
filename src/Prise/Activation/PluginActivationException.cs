using System;
using System.Runtime.Serialization;

namespace Prise.Activation
{
    [Serializable]
    public class PluginActivationException : Exception
    {
        public PluginActivationException(string message) : base(message)
        {
        }

        public PluginActivationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PluginActivationException()
        {
        }

        protected PluginActivationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}