
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.Example.Contract;
using Newtonsoft.Json;

namespace Prise.Example.Plugin.FileSystem
{
    [JsonObject]
    public class DataFile : List<MyDto> { }
    public interface IFileSystemService
    {
        Task<MyDto> CreateOnFileSystem(MyDto dto);
        Task<IEnumerable<MyDto>> ReadAllFromFileSystem();
    }

    public class FileSystemService : IFileSystemService
    {
        private readonly IConfigurationService configurationService;
        private string dataPath;
        private string dataFileName;

        public FileSystemService(IConfigurationService configurationService)
        {
            this.dataPath = configurationService.GetConfigurationValueForKey("FileSystem:DataPath");
            this.dataFileName = configurationService.GetConfigurationValueForKey("FileSystem:DataFileName");
        }

        public async Task<MyDto> CreateOnFileSystem(MyDto dto)
        {
            var dataFile = await DeserializeDataFile();

            dataFile.Add(dto);

            await SerializeDataFile(dataFile);

            return dto;
        }

        public async Task<IEnumerable<MyDto>> ReadAllFromFileSystem()
        {
            return await DeserializeDataFile();
        }

        private async Task<DataFile> DeserializeDataFile()
        {
            var fullPathToDataFile = Path.Combine(this.dataPath, this.dataFileName);
            if (!File.Exists(fullPathToDataFile))
                throw new System.Exception($"Data file {fullPathToDataFile} does not exist!");

            var file = await File.ReadAllTextAsync(fullPathToDataFile, System.Text.Encoding.UTF8);
            return JsonConvert.DeserializeObject<DataFile>(file);
        }

        private async Task SerializeDataFile(DataFile data)
        {
            var fullPathToDataFile = Path.Combine(this.dataPath, this.dataFileName);
            var json = JsonConvert.SerializeObject(data);
            await File.WriteAllTextAsync(fullPathToDataFile, json, System.Text.Encoding.UTF8);
        }
    }
}