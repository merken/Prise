var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");

private void CleanProject(string projectDirectory){
    var projectFile = $"{projectDirectory}/{projectDirectory}.csproj";
    var bin = $"{projectDirectory}/bin";
    var obj = $"{projectDirectory}/obj";

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
  CleanProject("OldSQLPlugin");
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

    DotNetCoreBuild("CosmosDbPlugin/CosmosDbPlugin.csproj", settings);
    DotNetCoreBuild("OldSQLPlugin/OldSQLPlugin.csproj", settings);
    DotNetCoreBuild("SQLPlugin/SQLPlugin.csproj", settings);
    DotNetCoreBuild("TableStoragePlugin/TableStoragePlugin.csproj", settings);
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    DotNetCorePublish("CosmosDbPlugin/CosmosDbPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/CosmosDbPlugin"
    });

    DotNetCorePublish("OldSQLPlugin/OldSQLPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/OldSQLPlugin"
    });

    DotNetCorePublish("SQLPlugin/SQLPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/SQLPlugin"
    });

    DotNetCorePublish("TableStoragePlugin/TableStoragePlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/TableStoragePlugin"
    });
  });

Task("copy-to-apphost")
  .IsDependentOn("publish")
  .Does(() =>
  {
    CopyDirectory("publish/CosmosDbPlugin", "MyHost/bin/debug/netcoreapp3.0/Plugins/CosmosDbPlugin");
    CopyDirectory("publish/CosmosDbPlugin", "MyHost2/bin/debug/netcoreapp2.1/Plugins/CosmosDbPlugin");
    CopyDirectory("publish/CosmosDbPlugin", "PluginServer/Plugins/CosmosDbPlugin");
    CopyDirectory("publish/SQLPlugin", "MyHost/bin/debug/netcoreapp3.0/Plugins/SQLPlugin");
    CopyDirectory("publish/SQLPlugin", "MyHost2/bin/debug/netcoreapp2.1/Plugins/SQLPlugin");
    CopyDirectory("publish/SQLPlugin", "PluginServer/Plugins/SQLPlugin");
    CopyDirectory("publish/OldSQLPlugin", "MyHost/bin/debug/netcoreapp3.0/Plugins/OldSQLPlugin");
    CopyDirectory("publish/OldSQLPlugin", "MyHost2/bin/debug/netcoreapp2.1/Plugins/OldSQLPlugin");
    CopyDirectory("publish/TableStoragePlugin", "MyHost/bin/debug/netcoreapp3.0/Plugins/TableStoragePlugin");
    CopyDirectory("publish/TableStoragePlugin", "MyHost2/bin/debug/netcoreapp2.1/Plugins/TableStoragePlugin");
    CopyDirectory("publish/TableStoragePlugin", "PluginServer/Plugins/TableStoragePlugin");
  });

Task("default")
  .IsDependentOn("copy-to-apphost");

RunTarget(target);