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
  var path = $"publish/{target}";
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
    // var projectFile = $"IntegrationTestsPlugins/{projectDirectory}/{projectDirectory}.csproj";
    var bin = $"IntegrationTestsPlugins/{projectDirectory}/bin";
    var obj = $"IntegrationTestsPlugins/{projectDirectory}/obj";

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

Task("clean").Does( () =>
{ 
  foreach (var plugin in plugins)
  {
    var projects = GetAllProjectsFromPluginDir($"IntegrationTestsPlugins/{plugin}");
    foreach (var project in projects)
      CleanProject(plugin, project);
  }
});

private void FixAssemblyIssues(string pathToDlls)
{
  using(var process = StartAndReturnProcess("dotnet", new ProcessSettings{ Arguments = $"FileBulkDateChanger/FileBulkDateChanger.dll {pathToDlls}" }))
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
      var projects = GetAllProjectsFromPluginDir($"IntegrationTestsPlugins/{plugin}");
      foreach (var project in projects)
      {
        var targetFramework = GetTargetFrameworkFromProject(project);
        var nuspecForProject = GetNuspecFileForProject(project);
        var outputDir = $"publish/{targetFramework}/{plugin}";
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
            FixAssemblyIssues($"IntegrationTestsPlugins/{plugin}/bin/{configuration}/{targetFramework}/publish");
            var nuspecFilename = System.IO.Path.GetFileName(nuspecForProject);
            DotNetCorePack(project, new DotNetCorePackSettings
            {
                NoBuild = true,
                Configuration = configuration,
                OutputDirectory = $"publish/{targetFramework}",
                ArgumentCustomization = (args) => args.Append($"/p:nuspecfile={nuspecFilename}")
            });
        }
    }
  }
});

Task("copy-to-testhosts")
  .IsDependentOn("publish")
  .Does(() =>
  {
    foreach(var netstandard20 in GetPublishedPlugins("netstandard2.0")){
      CopyDirectory(netstandard20, $"Prise.IntegrationTests/bin/debug/netcoreapp2.1/Plugins");
      CopyDirectory(netstandard20, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp2.1/Plugins");
      CopyDirectory(netstandard20, $"Prise.IntegrationTests/bin/debug/netcoreapp3.0/Plugins");
      CopyDirectory(netstandard20, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.0/Plugins");
      CopyDirectory(netstandard20, $"Prise.IntegrationTests/bin/debug/netcoreapp3.1/Plugins");
      CopyDirectory(netstandard20, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.1/Plugins");
    }
    foreach(var netstandard21 in GetPublishedPlugins("netstandard2.1")){
      CopyDirectory(netstandard21, $"Prise.IntegrationTests/bin/debug/netcoreapp2.1/Plugins");
      CopyDirectory(netstandard21, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp2.1/Plugins");
      CopyDirectory(netstandard21, $"Prise.IntegrationTests/bin/debug/netcoreapp3.0/Plugins");
      CopyDirectory(netstandard21, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.0/Plugins");
      CopyDirectory(netstandard21, $"Prise.IntegrationTests/bin/debug/netcoreapp3.1/Plugins");
      CopyDirectory(netstandard21, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.1/Plugins");
    }
    foreach(var netcoreapp21 in GetPublishedPlugins("netcoreapp2.1")){
      CopyDirectory(netcoreapp21, $"Prise.IntegrationTests/bin/debug/netcoreapp2.1/Plugins");
      CopyDirectory(netcoreapp21, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp2.1/Plugins");
      CopyDirectory(netcoreapp21, $"Prise.IntegrationTests/bin/debug/netcoreapp3.0/Plugins");
      CopyDirectory(netcoreapp21, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.0/Plugins");
      CopyDirectory(netcoreapp21, $"Prise.IntegrationTests/bin/debug/netcoreapp3.1/Plugins");
      CopyDirectory(netcoreapp21, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.1/Plugins");
    }
    foreach(var netcoreapp30 in GetPublishedPlugins("netcoreapp3.0")){
      CopyDirectory(netcoreapp30, $"Prise.IntegrationTests/bin/debug/netcoreapp3.0/Plugins");
      CopyDirectory(netcoreapp30, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.0/Plugins");
      CopyDirectory(netcoreapp30, $"Prise.IntegrationTests/bin/debug/netcoreapp3.1/Plugins");
      CopyDirectory(netcoreapp30, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.1/Plugins");
    }
    foreach(var netcoreapp31 in GetPublishedPlugins("netcoreapp3.1")){
      CopyDirectory(netcoreapp31, $"Prise.IntegrationTests/bin/debug/netcoreapp3.1/Plugins");
      CopyDirectory(netcoreapp31, $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.1/Plugins");
    }
  });

Task("default")
  .IsDependentOn("copy-to-testhosts");

RunTarget(target);