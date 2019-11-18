using System;
using System.Runtime.InteropServices;
using Prise.Infrastructure;

namespace Prise
{
#if NETCOREAPP2_1
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
#if NETCOREAPP3_0
            NativeLibrary.Free(pointerToAssembly);
#endif
#if NETCOREAPP2_1
            NativeAssemblyUnloader.FreeLibrary(pointerToAssembly);
#endif
        }
    }
}
