var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");

private void CleanProject(string projectDirectory){
    var projectFile = $"Plugins/{projectDirectory}/{projectDirectory}.csproj";
    var bin = $"Plugins/{projectDirectory}/bin";
    var obj = $"Plugins/{projectDirectory}/obj";

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

    DotNetCoreBuild("Plugins/HttpPlugin/HttpPlugin.csproj", settings);
    DotNetCoreBuild("Plugins/CosmosDbPlugin/CosmosDbPlugin.csproj", settings);
    DotNetCoreBuild("Plugins/OldSQLPlugin/OldSQLPlugin.csproj", settings);
    DotNetCoreBuild("Plugins/SQLPlugin/SQLPlugin.csproj", settings);
    DotNetCoreBuild("Plugins/TableStoragePlugin/TableStoragePlugin.csproj", settings);
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    DotNetCorePublish("Plugins/HttpPlugin/HttpPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/HttpPlugin"
    });


    DotNetCorePublish("Plugins/CosmosDbPlugin/CosmosDbPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/CosmosDbPlugin"
    });

    DotNetCorePublish("Plugins/OldSQLPlugin/OldSQLPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/OldSQLPlugin"
    });

    DotNetCorePublish("Plugins/SQLPlugin/SQLPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/SQLPlugin"
    });

    DotNetCorePublish("Plugins/TableStoragePlugin/TableStoragePlugin.csproj", new DotNetCorePublishSettings
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
    CopyDirectory("publish/HttpPlugin", "MyHost/bin/debug/netcoreapp3.0/Plugins/HttpPlugin");
    CopyDirectory("publish/HttpPlugin", "MyHost2/bin/debug/netcoreapp2.1/Plugins/HttpPlugin");
    CopyDirectory("publish/HttpPlugin", "PluginServer/Plugins/HttpPlugin");
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