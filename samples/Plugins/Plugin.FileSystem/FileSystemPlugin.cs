using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Example.Contract;
using Prise.Plugin;

namespace Plugin.FileSystem
{
    [Plugin(PluginType = typeof(IPlugin))]
    public class FileSystemPlugin : IPlugin
    {
        [PluginService(ProvidedBy = ProvidedBy.Plugin, ServiceType = typeof(IFileSystemService))]
        private readonly IFileSystemService fileSystemService;

        public async Task<MyDto> Create(int number, string text)
        {
            return await this.fileSystemService.CreateOnFileSystem(new MyDto
            {
                Number = number,
                Text = text
            });
        }

        public async Task<IEnumerable<MyDto>> GetAll()
        {
            return await this.fileSystemService.ReadAllFromFileSystem();
        }
    }
}
