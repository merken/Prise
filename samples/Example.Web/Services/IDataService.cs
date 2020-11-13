using Example.Contract;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Prise;


namespace Example.Web.Services
{
    public interface IDataService
    {
        Task<IEnumerable<MyDto>> GetDataFromAllPlugins();
    }

    public class DataService : IDataService
    {
        private readonly IPluginLoader pluginLoader;
        private readonly IHttpContextAccessorService httpContextAccessorService;
        private readonly IConfigurationService configurationService;
        public DataService(IPluginLoader pluginLoader, IHttpContextAccessorService httpContextAccessorService, IConfigurationService configurationService)
        {
            this.pluginLoader = pluginLoader;
            this.httpContextAccessorService = httpContextAccessorService;
            this.configurationService = configurationService;
        }

        public async Task<IEnumerable<MyDto>> GetDataFromAllPlugins()
        {
            var data = new List<MyDto>();
            var pluginResults = await this.pluginLoader.FindPlugins<IPlugin>(GetPathToDist());

            foreach (var pluginResult in pluginResults)
            {
                await foreach (var plugin in this.pluginLoader.LoadPluginsAsAsyncEnumerable<IPlugin>(pluginResult, configure: (context) =>
                    {
                        context.AddHostService<IHttpContextAccessorService>(this.httpContextAccessorService);
                        context.AddHostService<IConfigurationService>(this.configurationService);
                    }))
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
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Plugins/_dist"));
        }
    }
}