using Prise.Example.Contract;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace Prise.Example.Razor.Services
{
    public interface IDataService
    {
        Task<IEnumerable<MyDto>> GetDataFromAllIPlugins();
    }

    public class DataService : IDataService
    {
        private readonly IPluginLoader pluginLoader;
        public DataService(IPluginLoader pluginLoader)
        {
            this.pluginLoader = pluginLoader;
        }

        public async Task<IEnumerable<MyDto>> GetDataFromAllIPlugins()
        {
            var data = new List<MyDto>();
            var pluginResults = await this.pluginLoader.FindPlugins<IPlugin>(GetPathToDist());

            foreach (var pluginResult in pluginResults)
            {
                await foreach (var plugin in this.pluginLoader.LoadPlugins<IPlugin>(pluginResult))
                {
                    data.AddRange(await plugin.GetAll());
                }
            }

            return data;
        }

        private static string GetPathToDist()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Plugins/dist"));
        }
    }
}