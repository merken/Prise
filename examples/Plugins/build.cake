var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");

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

Task("clean").Does( () =>
{ 
  CleanProject("PluginA");
  CleanProject("PluginB");
  CleanProject("PluginC");
});

Task("build")
  .IsDependentOn("clean")
  .Does( () =>
{ 
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration
    };

    DotNetCoreBuild("PluginA\\PluginA.csproj", settings);
    DotNetCoreBuild("PluginB\\PluginB.csproj", settings);
    DotNetCoreBuild("PluginC\\PluginC.csproj", settings);
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    DotNetCorePublish("PluginA\\PluginA.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/PluginA"
    });

    DotNetCorePublish("PluginB\\PluginB.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/PluginB"
    });
    DotNetCorePublish("PluginC\\PluginC.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/PluginC"
    });

    CopyDirectory("publish/PluginA", "../PluginApp/Plugins/PluginA");
    CopyDirectory("publish/PluginA", "../PluginServer/Plugins/PluginA");
    CopyDirectory("publish/PluginB", "../PluginApp/Plugins/PluginB");
    CopyDirectory("publish/PluginB", "../PluginServer/Plugins/PluginB");
    CopyDirectory("publish/PluginC", "../PluginApp/Plugins/PluginC");
    CopyDirectory("publish/PluginC", "../PluginServer/Plugins/PluginC");
  });

Task("default")
  .IsDependentOn("publish");

RunTarget(target);