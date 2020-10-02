namespace Example.Contract
{
    public interface IMvcPlugin
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class MvcPluginDescriptionAttribute : System.Attribute
    {
        string description;
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }
    }
}