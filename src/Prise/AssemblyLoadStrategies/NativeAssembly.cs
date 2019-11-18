using System;

namespace Prise
{
    /// <summary>
    /// Represents a native library through either its full Path or a Pointer to an assembly in memory
    /// </summary>
    public class NativeAssembly
    {
        public string Path { get; private set; }
        public IntPtr Pointer { get; private set; }

        public static NativeAssembly Create(string path, IntPtr pointer) => new NativeAssembly { Path = path, Pointer = pointer };
    }
}
