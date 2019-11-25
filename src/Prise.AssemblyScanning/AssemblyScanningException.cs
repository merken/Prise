using System;
using System.Collections.Generic;
using System.Text;

namespace Prise.AssemblyScanning
{
    public class AssemblyScanningException : Exception
    {
        public AssemblyScanningException(string message) : base(message)
        {
        }

        public AssemblyScanningException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
