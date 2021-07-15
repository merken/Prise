using System;

namespace Prise.AssemblyLoading
{
#if !SUPPORTS_NATIVE_UNLOADING
#endif

    public class DefaultNativeAssemblyUnloader : INativeAssemblyUnloader
    {
        public void UnloadNativeAssembly(string fullPathToLoadedNativeAssembly, IntPtr pointerToAssembly)
        {
#if !SUPPORTS_NATIVE_UNLOADING
            NativeAssemblyUnloader.FreeLibrary(pointerToAssembly);
#else
            NativeLibrary.Free(pointerToAssembly);
#endif
        }
    }
}