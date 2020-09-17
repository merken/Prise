using System;

namespace Prise.AssemblyLoading
{
    public interface INativeAssemblyUnloader
    {
        void UnloadNativeAssembly(string fullPathToLoadedNativeAssembly, IntPtr pointerToAssembly);
    }
}