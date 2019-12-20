using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Prise.AssemblyScanning
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
