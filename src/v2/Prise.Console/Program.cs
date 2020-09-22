using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Prise.AssemblyScanning;
using Prise.Console.Contract;
using Prise.DependencyInjection;

namespace Prise.Console
{
    class Program
    {

        private static async Task<List<Core.AssemblyScanResult>> ScanAssemblies(string startingPath, Type pluginType, bool scanNugets)
        {
            using (var scanner = DefaultFactories.DefaultAssemblyScanner())
            using (var nugetScanner = DefaultFactories.DefaultNuGetAssemblyScanner())
            {
                var results = await scanner.Scan(new AssemblyScannerOptions
                {
                    StartingPath = startingPath,
                    PluginType = pluginType
                });

                var nugetResults = scanNugets ? await nugetScanner.Scan(new AssemblyScannerOptions
                {
                    StartingPath = startingPath,
                    PluginType = pluginType
                }) : Enumerable.Empty<Core.AssemblyScanResult>();

                return results.Union(nugetResults).ToList();
            }
        }

        static async Task Main(string[] args)
        {
            var type = typeof(IPlugin);
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            var pathToDist = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist"));
            // Purge the nuget extraction dir
            var extractedDir = Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist/_extracted"));
            if (Directory.Exists(extractedDir))
                Directory.Delete(Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Packages/dist/_extracted")), true);

            var allResults = await ScanAssemblies(pathToDist, type, false);

            System.Console.WriteLine($"Scanning results");
            foreach (var result in allResults
                           .OrderBy(r => r.AssemblyPath)
                           .ThenBy(a => a.AssemblyName))
                System.Console.WriteLine($"{result.AssemblyName} {result.AssemblyPath}");

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

                    var options = allResults//.Union(nugetResults)
                                        .OrderBy(r => r.AssemblyPath)
                                        .ThenBy(a => a.AssemblyName).ToList();
                    System.Console.WriteLine($"");
                    System.Console.WriteLine($"Load which plugin assembly?");
                    System.Console.WriteLine($"---------------------------");
                    foreach (var option in options)
                        System.Console.WriteLine($"{options.IndexOf(option) + 1}: {option.AssemblyName} {option.AssemblyPath}");

                    var inputString = System.Console.ReadLine();
                    if (inputString.ToLower() == "r")
                    {
                        // rescan
                        allResults = await ScanAssemblies(pathToDist, type, false);
                        input = 99;
                        continue;

                    }
                    var parsed = int.TryParse(inputString, out input);
                    if (!parsed || input < 0 || input > options.Count() + 1)
                    {
                        System.Console.WriteLine($"Invalid input {inputString}");
                        break;
                    }

                    var hostFramework = Utils.HostFrameworkUtils.GetHostframeworkFromType(typeof(Program));

                    var optionToLoad = options.ElementAt(input - 1);
                    using (var loader = DefaultFactories.DefaultAssemblyLoader())
                    using (var activator = DefaultFactories.DefaultPluginActivator())
                    {
                        var pathToAssembly = Path.Combine(optionToLoad.AssemblyPath, optionToLoad.AssemblyName);

                        var pluginLoadContext = Prise.Core.PluginLoadContext.DefaultPluginLoadContext(pathToAssembly, typeof(IPlugin), hostFramework);
                        // This allows the loading of netstandard plugins
                        pluginLoadContext.IgnorePlatformInconsistencies = true;

                        var pluginAssembly = await loader.Load(pluginLoadContext);

                        messages.AppendLine($"Assembly {pluginAssembly.Assembly.FullName} {optionToLoad.AssemblyPath} loaded!");

                        var pluginTypeSelector = DefaultFactories.DefaultPluginTypeSelector();

                        var pluginTypes = pluginTypeSelector.SelectPluginTypes<IPlugin>(pluginAssembly);
                        var firstPlugin = pluginTypes.FirstOrDefault();

                        var pluginInstance = await activator.ActivatePlugin<IPlugin>(new Activation.DefaultPluginActivationOptions
                        {
                            PluginType = firstPlugin,
                            PluginAssembly = pluginAssembly,
                            ParameterConverter = DefaultFactories.DefaultParameterConverter(),
                            ResultConverter = DefaultFactories.DefaultResultConverter()
                        });
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
