using System;

namespace Prise.Infrastructure
{
    public interface INativeAssemblyUnloader
    {
        void UnloadNativeAssembly(string fullPathToLoadedNativeAssembly, IntPtr pointerToAssembly);
    }
}
