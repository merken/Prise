var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");

private void CleanProject(string projectDirectory){
    var projectFile = $"{projectDirectory}";
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
  CleanProject("Plugins\\PluginA");
  CleanProject("Plugins\\PluginB");
  CleanProject("Plugins\\PluginC");
});

Task("build")
  .IsDependentOn("clean")
  .Does( () =>
{ 
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration
    };

    DotNetCoreBuild("Plugins\\PluginA\\PluginA.csproj", settings);
    DotNetCoreBuild("Plugins\\PluginB\\PluginB.csproj", settings);
    DotNetCoreBuild("Plugins\\PluginC\\PluginC.csproj", settings);
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    DotNetCorePublish("Plugins\\PluginA\\PluginA.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/PluginA"
    });

    DotNetCorePublish("Plugins\\PluginB\\PluginB.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/PluginB"
    });
    DotNetCorePublish("Plugins\\PluginC\\PluginC.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/PluginC"
    });

    CopyDirectory("publish/PluginA", "AppHost/bin/debug/netcoreapp3.0/Plugins/PluginA");
    CopyDirectory("publish/PluginA", "Tests/bin/debug/netcoreapp3.0/Plugins/PluginA");
    CopyDirectory("publish/PluginB", "AppHost/bin/debug/netcoreapp3.0/Plugins/PluginB");
    CopyDirectory("publish/PluginB", "Tests/bin/debug/netcoreapp3.0/Plugins/PluginB");
    CopyDirectory("publish/PluginC", "AppHost/bin/debug/netcoreapp3.0/Plugins/PluginC");
    CopyDirectory("publish/PluginC", "Tests/bin/debug/netcoreapp3.0/Plugins/PluginC");
  });

Task("default")
  .IsDependentOn("publish");

RunTarget(target);