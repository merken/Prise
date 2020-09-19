using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Prise.Console.Contract;
using Prise.Web;
namespace Prise.Console2
{
    
    class LoadContext : AssemblyLoadContext
    {

        internal static async Task<Stream> LoadFileFromLocalDisk(string fullPathToAssembly)
        {
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(fullPathToAssembly, FileMode.Open, FileAccess.Read))
            {
                memoryStream.SetLength(stream.Length);
                await stream.ReadAsync(memoryStream.GetBuffer(), 0, (int)stream.Length);
            }
            return memoryStream;
        }

        public async Task<Assembly> LoadPLugin(string path)
        {
            return base.LoadFromStream(await LoadFileFromLocalDisk(path));
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            System.Console.WriteLine($"Loaded {assembly.FullName} from {assembly.Location}");
            return assembly;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return base.LoadUnmanagedDll(unmanagedDllName);
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {

            var context = new LoadContext();
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                                  .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            var pathToPlugin = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/Prise.Plugin.Sql.Legacy/bin/Debug/netcoreapp2.1"));

            var pluginAssembly = Path.Combine(pathToPlugin, "Prise.Plugin.Sql.Legacy.dll");
            var assembly = await context.LoadPLugin(pluginAssembly);
            var type = assembly.GetType("Prise.Plugin.Sql.Legacy.SqlStoragePlugin");
            var instance = assembly.CreateInstance(type.FullName, true);
            instance.GetType().GetTypeInfo()
                            .DeclaredFields
                                .First(f => f.Name == "configurationService")
                                .SetValue(instance, new AppSettingsConfigurationService());

            instance.GetType().GetTypeInfo().GetMethod("OnActivated").Invoke(instance, null);

            var plugin = instance as IStoragePlugin;

            // TODO THIS WORKS BUT PLATFORM UNSUPPORTED ON NET CORE 2????
            System.Console.WriteLine(plugin.GetData(new Console.Contract.PluginObject
            {
                Text = "999"
            }).Result.Text);

        }
    }
}
