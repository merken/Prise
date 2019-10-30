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
  CleanProject("CosmosDbPlugin");
  CleanProject("SQLPlugin");
  CleanProject("TableStoragePlugin");
});

Task("build")
  .IsDependentOn("clean")
  .Does( () =>
{ 
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration
    };

    DotNetCoreBuild("CosmosDbPlugin\\CosmosDbPlugin.csproj", settings);
    DotNetCoreBuild("SQLPlugin\\SQLPlugin.csproj", settings);
    DotNetCoreBuild("TableStoragePlugin\\TableStoragePlugin.csproj", settings);
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    DotNetCorePublish("CosmosDbPlugin\\CosmosDbPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/CosmosDbPlugin"
    });

    DotNetCorePublish("SQLPlugin\\SQLPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/SQLPlugin"
    });
    DotNetCorePublish("TableStoragePlugin\\TableStoragePlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/TableStoragePlugin"
    });

    CopyDirectory("publish/CosmosDbPlugin", "AppHost/bin/debug/netcoreapp3.0/Plugins/CosmosDbPlugin");
    CopyDirectory("publish/SQLPlugin", "AppHost/bin/debug/netcoreapp3.0/Plugins/SQLPlugin");
    CopyDirectory("publish/TableStoragePlugin", "AppHost/bin/debug/netcoreapp3.0/Plugins/TableStoragePlugin");
  });

Task("default")
  .IsDependentOn("publish");

RunTarget(target);