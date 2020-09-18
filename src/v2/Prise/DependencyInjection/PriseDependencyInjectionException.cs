using System;
using System.Runtime.Serialization;

namespace Prise.DependencyInjection
{
    [Serializable]
    public class PriseDependencyInjectionException : Exception
    {
        public PriseDependencyInjectionException(string message) : base(message)
        {
        }

        public PriseDependencyInjectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PriseDependencyInjectionException()
        {
        }

        protected PriseDependencyInjectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}