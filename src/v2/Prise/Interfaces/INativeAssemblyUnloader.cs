using System;

namespace Prise.V2
{
    public interface INativeAssemblyUnloader
    {
        void UnloadNativeAssembly(string fullPathToLoadedNativeAssembly, IntPtr pointerToAssembly);
    }
}