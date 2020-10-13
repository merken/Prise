namespace Prise.Platform
{
    public interface IPlatformAbstraction
    {
        bool IsLinux();
        bool IsOSX();
        bool IsWindows();
    }

    public class DefaultPlatformAbstraction : IPlatformAbstraction
    {
        public bool IsLinux() => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);

        public bool IsOSX() => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);

        public bool IsWindows() => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
    }
}