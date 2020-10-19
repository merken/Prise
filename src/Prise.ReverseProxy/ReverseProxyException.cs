namespace Prise.Proxy
{
    [System.Serializable]
    public class ReverseProxyException : System.Exception
    {
        public ReverseProxyException() { }
        public ReverseProxyException(string message) : base(message) { }
        public ReverseProxyException(string message, System.Exception inner) : base(message, inner) { }
        protected ReverseProxyException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}