namespace Prise
{
    [System.Serializable]
    public class PluginLoadException : System.Exception
    {
        public PluginLoadException() { }
        public PluginLoadException(string message) : base(message) { }
        public PluginLoadException(string message, System.Exception inner) : base(message, inner) { }
        protected PluginLoadException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}