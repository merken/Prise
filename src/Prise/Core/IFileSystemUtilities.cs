using System.IO;
using System.Threading.Tasks;

namespace Prise
{
    public interface IFileSystemUtilities
    {
        bool DoesFileExist(string pathToFile);
        Task<Stream> ReadFileFromDisk(string pathToFile);
        string EnsureFileExists(string pathToFile);
        Stream ReadDependencyFileFromDisk(string loadPath, string pluginAssemblyName);
        string EnsureDependencyFileExists(string loadPath, string pluginAssemblyName);
        byte[] ToByteArray(Stream stream);
    }
}