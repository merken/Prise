using System.Reflection;

namespace Prise.AssemblyLoading
{
    public class AssemblyFromStrategy
    {
        public Assembly Assembly { get; set; }
        public bool CanBeReleased { get; set; }

        public static AssemblyFromStrategy Releasable(Assembly assembly) => new AssemblyFromStrategy() { Assembly = assembly, CanBeReleased = true };
        public static AssemblyFromStrategy NotReleasable(Assembly assembly) => new AssemblyFromStrategy() { Assembly = assembly, CanBeReleased = false };
    }
}