using System.Reflection;

namespace Prise
{
    public interface IAssemblyShim
    {
        Assembly Assembly { get; }
    }

    public class PriseAssembly : IAssemblyShim
    {
        public PriseAssembly(Assembly assembly)
        {
            this.Assembly = assembly;
        }
        
        public Assembly Assembly { get; private set; }
    }
}