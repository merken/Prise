using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Prise.Console.Contract;

namespace Prise.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var type = typeof(IPlugin);
            var scanner = new Prise.V2.DefaultAssemblyScanner();
            var nugetScanner = new Prise.V2.NugetPackageAssemblyScanner();

            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            var pathToDist = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist"));
            // Purge the nuget extraction dir
            var extractedDir = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist/_extracted"));
            if (Directory.Exists(extractedDir))
                Directory.Delete(Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist/_extracted")), true);

            var results = await scanner.Scan(pathToDist, type);
            // var nugetResults = await nugetScanner.Scan(pathToDist, type);

            System.Console.WriteLine($"Scanning results");
            foreach (var result in results
                           .OrderBy(r => r.AssemblyPath)
                           .ThenBy(a => a.AssemblyName))
                System.Console.WriteLine($"{result.AssemblyName} {result.AssemblyPath}");

            // System.Console.WriteLine($"Nuget Scanning results");
            // foreach (var result in nugetResults
            //            .OrderBy(r => r.AssemblyPath)
            //            .ThenBy(a => a.AssemblyName))
            //     System.Console.WriteLine($"{result.AssemblyName} {result.AssemblyPath}");

            var input = 0;
            var error = String.Empty;
            var messages = new StringBuilder();
            do
            {
                try
                {
                    System.Console.Clear();

                    if (!String.IsNullOrEmpty(error))
                    {
                        System.Console.WriteLine($"-----------ERROR------------");
                        System.Console.WriteLine(error);
                        System.Console.WriteLine($"---------------------------");
                    }

                    if (!String.IsNullOrEmpty(messages.ToString()))
                    {
                        System.Console.WriteLine($"-----------MESSAGES---------");
                        System.Console.WriteLine(messages.ToString());
                        System.Console.WriteLine($"---------------------------");
                    }

                    var options = results//.Union(nugetResults)
                                        .OrderBy(r => r.AssemblyPath)
                                        .ThenBy(a => a.AssemblyName).ToList();
                    System.Console.WriteLine($"");
                    System.Console.WriteLine($"Load which plugin assembly?");
                    System.Console.WriteLine($"---------------------------");
                    foreach (var option in options)
                        System.Console.WriteLine($"{options.IndexOf(option) + 1}: {option.AssemblyName} {option.AssemblyPath}");

                    var inputString = System.Console.ReadLine();
                    var parsed = int.TryParse(inputString, out input);
                    if (!parsed || input < 0 || input > options.Count() + 1)
                    {
                        System.Console.WriteLine($"Invalid input {inputString}");
                        break;
                    }

                    var optionToLoad = options.ElementAt(input - 1);
                    using (var loader = new Prise.V2.DefaultAssemblyLoader())
                    using (var activator = new Prise.V2.DefaultPluginActivator())
                    {
                        var pathToAssembly = Path.Combine(optionToLoad.AssemblyPath, optionToLoad.AssemblyName);
                        var pluginLoadContext = Prise.V2.PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(IPlugin), ignorePlatformInconsistencies: true);
                        var pluginAssembly = await loader.Load(pluginLoadContext);

                        messages.AppendLine($"Assembly {pluginAssembly.Assembly.FullName} {optionToLoad.AssemblyPath} loaded!");

                        var pluginTypeSelector = new Prise.V2.DefaultPluginTypeSelector();

                        var pluginTypes = pluginTypeSelector.SelectPluginTypes<IPlugin>(pluginAssembly);
                        var firstPlugin = pluginTypes.FirstOrDefault();

                        var pluginInstance = await activator.ActivatePlugin<IPlugin>(pluginAssembly, firstPlugin);
                        messages.AppendLine((await pluginInstance.GetData(new PluginObject { Text = "Plugin says " })).Text);
                        await loader.Unload(pluginLoadContext);
                    }
                }
                catch (Exception ex)
                {
                    error = $"{ex.Message} {ex.StackTrace}";
                }
            } while (input != 0);
        }
    }
}
