using System.Reflection;

namespace Prise
{
    public interface IAssemblyShim
    {
        Assembly Assembly { get; }
    }
}