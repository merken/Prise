using System;
using System.Runtime.InteropServices;

namespace Prise.AssemblyLoading
{
    internal static class NativeAssemblyUnloader
    {
        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);
    }
}