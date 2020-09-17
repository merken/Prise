using System;
using System.Runtime.Serialization;

namespace Prise.AssemblyLoading
{
    [Serializable]
    public class AssemblyLoadException : Exception
    {
        public AssemblyLoadException(string message) : base(message)
        {
        }

        public AssemblyLoadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AssemblyLoadException()
        {
        }

        protected AssemblyLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}