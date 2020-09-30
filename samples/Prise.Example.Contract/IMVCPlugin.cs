namespace Prise.Example.Contract
{
    public interface IMVCPlugin
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class MVCFeatureDescriptionAttribute : System.Attribute
    {
        string description;
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }
    }
}