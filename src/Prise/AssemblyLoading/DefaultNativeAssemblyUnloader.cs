using System;
using System.Runtime.InteropServices;

namespace Prise.AssemblyLoading
{
#if !SUPPORTS_NATIVE_UNLOADING
    internal static class NativeAssemblyUnloader
    {
        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);
    }
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