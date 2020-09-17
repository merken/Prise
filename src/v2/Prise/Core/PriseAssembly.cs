using System.Reflection;

namespace Prise
{
    public class PriseAssembly : IAssemblyShim
    {
        public PriseAssembly(Assembly assembly)
        {
            this.Assembly = assembly;
        }
        
        public Assembly Assembly { get; private set; }
    }
}