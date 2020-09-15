using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Prise.Console
{
    public class PluginObject
    {
        public int Number { get; set; }
        public string Text { get; set; }
    }

    public interface IPlugin
    {
        Task<PluginObject> GetData(PluginObject input);
    }

    class Program
    {
        static async Task Main(string[] args)
        {

            var type = typeof(IPlugin);
            var scanner = new DefaultAssemblyScanner();
            var nugetScanner = new NugetPackageAssemblyScanner();

            var results = await scanner.Scan(Path.Combine("packages"), type);
            var nugetResults = await nugetScanner.Scan(Path.Combine("packages"), type);

            foreach (var result in nugetResults
                           .OrderBy(r => r.AssemblyPath)
                           .ThenBy(a => a.AssemblyName))
            {
                var pluginAssembly = await pluginLoadOptions.AssemblyLoader.LoadAsync(loadContext);
                this.pluginAssemblies.Add(pluginAssembly);
                var pluginInstances = CreatePluginInstances(pluginLoadOptions, ref pluginAssembly);
                instances.AddRange(pluginInstances);
            }

            System.Console.WriteLine("Hello World!");
        }
    }
}
