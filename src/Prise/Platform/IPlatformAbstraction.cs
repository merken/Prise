namespace Prise.Platform
{
    public interface IPlatformAbstraction
    {
        bool IsLinux();
        bool IsOSX();
        bool IsWindows();
    }
}