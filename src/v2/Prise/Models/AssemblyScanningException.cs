using System;
using System.Runtime.Serialization;

namespace Prise.V2
{
    [Serializable]
    public class AssemblyScanningException : Exception
    {
        public AssemblyScanningException(string message) : base(message)
        {
        }

        public AssemblyScanningException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AssemblyScanningException()
        {
        }

        protected AssemblyScanningException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}