using System;
using System.IO;
using System.Threading.Tasks;

namespace Prise
{
    public class DefaultFileSystemUtilities : IFileSystemUtilities
    {
        public bool DoesFileExist(string pathToFile)
        {
            return File.Exists(pathToFile);
        }

        public async Task<Stream> ReadFileFromDisk(string pathToFile)
        {
            var probingPath = EnsureFileExists(pathToFile);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(probingPath, FileMode.Open, FileAccess.Read))
            {
                memoryStream.SetLength(stream.Length);
                await stream.ReadAsync(memoryStream.GetBuffer(), 0, (int)stream.Length);
            }
            return memoryStream;
        }

        public string EnsureFileExists(string pathToFile)
        {
            var probingPath = Path.GetFullPath(pathToFile);
            if (!File.Exists(probingPath))
                throw new FileNotFoundException($"Plugin assembly does not exist in path : {probingPath}");
            return probingPath;
        }

        public Stream ReadDependencyFileFromDisk(string loadPath, string pluginAssemblyName)
        {
            var probingPath = EnsureDependencyFileExists(loadPath, pluginAssemblyName);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(probingPath, FileMode.Open, FileAccess.Read))
            {
                memoryStream.SetLength(stream.Length);
                stream.Read(memoryStream.GetBuffer(), 0, (int)stream.Length);
            }
            return memoryStream;
        }

        public byte[] ToByteArray(Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }

        public string EnsureDependencyFileExists(string loadPath, string pluginAssemblyName)
        {
            var probingPath = Path.GetFullPath(Path.Combine(loadPath, pluginAssemblyName));
            if (!File.Exists(probingPath))
                throw new FileNotFoundException($"Plugin dependency assembly does not exist in path : {probingPath}");
            return probingPath;
        }
    }
}