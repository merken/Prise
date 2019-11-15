using System;

namespace Prise.Infrastructure.NetCore
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
