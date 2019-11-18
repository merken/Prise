using System;

namespace Prise
{
    public class PrisePluginException : Exception
    {
        public PrisePluginException(string message) : base(message)
        {
        }

        public PrisePluginException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
