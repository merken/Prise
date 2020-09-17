using System;
using System.Runtime.InteropServices;

namespace Prise.AssemblyLoading
{
    public class DefaultNativeAssemblyUnloader : INativeAssemblyUnloader
    {
        public void UnloadNativeAssembly(string fullPathToLoadedNativeAssembly, IntPtr pointerToAssembly)
        {
            NativeLibrary.Free(pointerToAssembly);
        }
    }
}