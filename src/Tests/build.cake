var target = Argument("target", "default");
var configuration = Argument("configuration", "Debug");
var plugins = new[] { 
  "LegacyPlugin1.4", 
  "LegacyPlugin1.5",
  "PluginA", 
  "PluginB",
  "PluginC", 
  "PluginCFromNetwork", 
  "PluginD", 
  "PluginDWithFactory", 
  "PluginE", 
  "PluginF", 
  "PluginG"
};
var networkPlugins = new[] { "PluginCFromNetwork" };

public string[] GetPublishedPlugins(string target){
  var path = System.IO.Path.Combine($"publish", $"{target}");
  if(System.IO.Directory.Exists(path))
    return System.IO.Directory.GetDirectories(path);
  return new string[0];
}

public string[] GetAllProjectsFromPluginDir(string dir)
{
  return System.IO.Directory.GetFiles(dir, "*.csproj");
}

public string GetNuspecFileForProject(string project)
{
  var dir = System.IO.Path.GetDirectoryName(project);
  var projectNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(project);
  var nuspecs = System.IO.Directory.GetFiles(dir, "*.nuspec");

  return nuspecs.FirstOrDefault(n => System.IO.Path.GetFileNameWithoutExtension(n) == projectNameWithoutExtension);
}

public string GetTargetFrameworkFromProject(string project)
{
  var targetFramework = System.Xml.Linq.XDocument.Load(project).Root.DescendantNodes().OfType<System.Xml.Linq.XElement>()
      .FirstOrDefault(x => x.Name.LocalName.Equals("TargetFramework"));

  return targetFramework?.Value;
}

private void CleanProject(string projectDirectory, string projectFile){
    var bin = System.IO.Path.Combine($"IntegrationTestsPlugins", $"{projectDirectory}", "bin");
    var obj = System.IO.Path.Combine($"IntegrationTestsPlugins", $"{projectDirectory}", "obj");

    var deleteSettings = new DeleteDirectorySettings{
        Force= true,
        Recursive = true
    };

    var cleanSettings = new DotNetCoreCleanSettings
    {
        Configuration = configuration
    };
    if (DirectoryExists(bin))
    {
      DeleteDirectory(bin, deleteSettings);
    }
    if (DirectoryExists(obj))
    {
      DeleteDirectory(obj, deleteSettings);
    }
    DotNetCoreClean(projectFile, cleanSettings);
}

Task("clean")
  .Does( () =>
  { 
    foreach (var plugin in plugins)
    {
      var projects = GetAllProjectsFromPluginDir($"IntegrationTestsPlugins/{plugin}");
      foreach (var project in projects)
        CleanProject(plugin, project);
    }
  });

Task("clean-published")
  .Does( () =>
  { 
    var deleteSettings = new DeleteDirectorySettings{
        Force= true,
        Recursive = true
    };
    DeleteDirectory("publish", deleteSettings);
    DeleteDirectory(System.IO.Path.Combine($"Prise.IntegrationTests", "bin", "debug","netcoreapp2.1","Plugins"), deleteSettings);
    DeleteDirectory(System.IO.Path.Combine($"Prise.IntegrationTestsHost", "bin", "debug","netcoreapp2.1","Plugins"), deleteSettings);
    DeleteDirectory(System.IO.Path.Combine($"Prise.IntegrationTests", "bin", "debug","netcoreapp3.0","Plugins"), deleteSettings);
    DeleteDirectory(System.IO.Path.Combine($"Prise.IntegrationTestsHost", "bin", "debug","netcoreapp3.0","Plugins"), deleteSettings);
    DeleteDirectory(System.IO.Path.Combine($"Prise.IntegrationTests", "bin", "debug","netcoreapp3.1","Plugins"), deleteSettings);
    DeleteDirectory(System.IO.Path.Combine($"Prise.IntegrationTestsHost", "bin", "debug","netcoreapp3.1","Plugins"), deleteSettings);
  });

private void FixAssemblyIssues(string pathToDlls)
{
  using(var process = StartAndReturnProcess("dotnet", 
    new ProcessSettings{ Arguments = $"{System.IO.Path.Combine("FileBulkDateChanger", "FileBulkDateChanger.dll")} {pathToDlls}"}))
  {
      process.WaitForExit(2000);//Waits for 2 seconds
  }
}

Task("publish")
  .IsDependentOn("clean")
  .Does(() =>
  { 
    foreach (var plugin in plugins)
    {
      var projects = GetAllProjectsFromPluginDir(System.IO.Path.Combine("IntegrationTestsPlugins", $"{plugin}"));
      foreach (var project in projects)
      {
        var targetFramework = GetTargetFrameworkFromProject(project);
        var nuspecForProject = GetNuspecFileForProject(project);
        var outputDir = System.IO.Path.Combine("publish", $"{targetFramework}", $"{plugin}");
        if(nuspecForProject!= null)
          outputDir = null;
        
        DotNetCorePublish(project, new DotNetCorePublishSettings
        {
            NoBuild = false,
            Configuration = configuration,
            Framework = targetFramework,
            OutputDirectory = outputDir
        });

        if(nuspecForProject!= null){
            FixAssemblyIssues(System.IO.Path.Combine($"IntegrationTestsPlugins", $"{plugin}", "bin", $"{configuration}", $"{targetFramework}", "publish"));
            var nuspecFilename = System.IO.Path.GetFileName(nuspecForProject);
            DotNetCorePack(project, new DotNetCorePackSettings
            {
                NoBuild = true,
                Configuration = configuration,
                OutputDirectory = System.IO.Path.Combine($"publish", $"{targetFramework}"),
                ArgumentCustomization = (args) => args.Append($"/p:nuspecfile={nuspecFilename}")
            });
        }
    }
  }
});

private void CopyToTestHosts(string dir, params string[] frameworks){
  var publishDir = System.IO.Path.GetDirectoryName(dir); // last directory in path: x/y/z => z
  var pluginDir = System.IO.Path.GetFileName(dir); // last directory in path: x/y/z => z
  foreach(var fwk in frameworks){
    CopyDirectory(dir, System.IO.Path.Combine($"Prise.IntegrationTests", "bin", "debug", fwk, "Plugins", pluginDir));
    CopyDirectory(dir, System.IO.Path.Combine($"Prise.IntegrationTestsHost", "bin", "debug", fwk, "Plugins", pluginDir));
  }
}

Task("copy-to-testhosts")
  .IsDependentOn("publish")
  .Does(() =>
  {
    foreach(var netstandard20 in GetPublishedPlugins("netstandard2.0")){
    Console.WriteLine(netstandard20);
      if(System.IO.Directory.Exists(netstandard20))
        CopyToTestHosts(netstandard20, "netcoreapp2.1", "netcoreapp3.0", "netcoreapp3.1");
    }
    foreach(var netstandard21 in GetPublishedPlugins("netstandard2.1")){
      if(System.IO.Directory.Exists(netstandard21))
        CopyToTestHosts(netstandard21, "netcoreapp2.1", "netcoreapp3.0", "netcoreapp3.1");
    }
    foreach(var netcoreapp21 in GetPublishedPlugins("netcoreapp2.1")){
      if(System.IO.Directory.Exists(netcoreapp21))
        CopyToTestHosts(netcoreapp21, "netcoreapp2.1", "netcoreapp3.0", "netcoreapp3.1");
    }
    foreach(var netcoreapp30 in GetPublishedPlugins("netcoreapp3.0")){
      if(System.IO.Directory.Exists(netcoreapp30))
        CopyToTestHosts(netcoreapp30, "netcoreapp3.0", "netcoreapp3.1");
    }
    foreach(var netcoreapp31 in GetPublishedPlugins("netcoreapp3.1")){
      if(System.IO.Directory.Exists(netcoreapp31))
        CopyToTestHosts(netcoreapp31, "netcoreapp3.1");
    }

    foreach(var publishTarget in new[] { "netstandard2.0", "netstandard2.1", "netcoreapp2.1", "netcoreapp3.0", "netcoreapp3.1"}){
      var directory = System.IO.Path.Combine($"publish", $"{publishTarget}");
      if(!System.IO.Directory.Exists(directory))
        continue;

      var packages = System.IO.Directory.GetFiles(directory, "*.nupkg");
      foreach(var package in packages)
        foreach(var fwk in new[] {"netcoreapp2.1", "netcoreapp3.0", "netcoreapp3.1"}){
          CopyFiles(package, System.IO.Path.Combine($"Prise.IntegrationTests", "bin", "debug", fwk, "Plugins"));
          CopyFiles(package, System.IO.Path.Combine($"Prise.IntegrationTestsHost", "bin", "debug", fwk, "Plugins"));
        }
    }
  });

Task("default")
  .IsDependentOn("copy-to-testhosts");

RunTarget(target);