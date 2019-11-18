/// <summary>
/// This build scripts publishes all the plugins in this directory and copies the files over to the /Plugins directory of the PluginServer
/// </summary>
/// <returns></returns>
var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");

private void CleanProject(string projectDirectory){
    var projectFile = $"./Plugins/{projectDirectory}/{projectDirectory}.csproj";
    var bin = $"./Plugins/{projectDirectory}/bin";
    var obj = $"./Plugins/{projectDirectory}/obj";

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
    return GetDirectories("./**/*.Plugin").Select(d=>d.GetDirectoryName()).ToArray();
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
        DotNetCoreBuild($"./Plugins/{directory}/{directory}.csproj", settings);
    }
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    foreach(var directory in GetComponentDirectories())
    {
      DotNetCorePublish($"./Plugins/{directory}/{directory}.csproj", new DotNetCorePublishSettings
      {
          NoBuild = true,
          Configuration = configuration,
          OutputDirectory = $"publish/{directory}"
      });
    }
  });

Task("push-to-server")
  .IsDependentOn("publish")
  .Does(() =>
  { 
    foreach(var directory in GetComponentDirectories())
    {
      CopyDirectory($"publish/{directory}", $"./PluginServer/Plugins/{directory}");
    }
  });

Task("default")
  .IsDependentOn("push-to-server");

RunTarget(target);