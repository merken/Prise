/// <summary>
/// This build scripts publishes all the plugins in this directory and copies the files over to the /Plugins directory of the PluginServer
/// </summary>
/// <returns></returns>
var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var hostPluginsDirectory = "../MyHost/bin/Debug/netcoreapp3.0/Plugins";

private void CleanProject(string projectDirectory){
    var projectFile = $"{projectDirectory}\\{projectDirectory}.csproj";
    var bin = $"{projectDirectory}\\bin";
    var obj = $"{projectDirectory}\\obj";

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

private string[] GetComponentDirectories(){
    return GetDirectories("./*.Plugin").Select(d=>d.GetDirectoryName()).ToArray();
}

Task("clean").Does( () =>
{ 
    foreach(var directory in GetComponentDirectories())
    {
        CleanProject(directory);
    }
});

Task("build")
  .IsDependentOn("clean")
  .Does( () =>
{ 
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration
    };

    foreach(var directory in GetComponentDirectories())
    {
        DotNetCoreBuild($"{directory}\\{directory}.csproj", settings);
    }
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    foreach(var directory in GetComponentDirectories())
    {
      DotNetCorePublish($"{directory}\\{directory}.csproj", new DotNetCorePublishSettings
      {
          NoBuild = true,
          Configuration = configuration,
          OutputDirectory = $"publish/{directory}"
      });
    }
  });

Task("push-to-host")
  .IsDependentOn("publish")
  .Does(() =>
  { 
    foreach(var directory in GetComponentDirectories())
    {
      CopyDirectory($"publish/{directory}", $"{hostPluginsDirectory}/{directory}");
    }
  });

Task("default")
  .IsDependentOn("push-to-host");

RunTarget(target);