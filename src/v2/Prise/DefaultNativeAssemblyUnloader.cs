using System;
using System.Runtime.InteropServices;

namespace Prise.V2
{
    public class DefaultNativeAssemblyUnloader : INativeAssemblyUnloader
    {
        public void UnloadNativeAssembly(string fullPathToLoadedNativeAssembly, IntPtr pointerToAssembly)
        {
            NativeLibrary.Free(pointerToAssembly);
        }
    }
}