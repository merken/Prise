using System;
using System.Runtime.Serialization;

namespace Prise.AssemblyLoading
{
    [Serializable]
    public class AssemblyLoadingException : Exception
    {
        public AssemblyLoadingException(string message) : base(message)
        {
        }

        public AssemblyLoadingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AssemblyLoadingException()
        {
        }

        protected AssemblyLoadingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}