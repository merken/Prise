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

    DotNetCoreBuild("Plugins/OldSQLPlugin/OldSQLPlugin.csproj", settings);
    DotNetCoreBuild("Plugins/SQLPlugin/SQLPlugin.csproj", settings);
    DotNetCoreBuild("Plugins/TableStoragePlugin/TableStoragePlugin.csproj", settings);
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
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
    CopyDirectory("publish/SQLPlugin", "Products.API/bin/debug/netcoreapp3.0/Plugins/SQLPlugin");
    CopyDirectory("publish/OldSQLPlugin", "Products.API/bin/debug/netcoreapp3.0/Plugins/OldSQLPlugin");
    CopyDirectory("publish/TableStoragePlugin", "Products.API/bin/debug/netcoreapp3.0/Plugins/TableStoragePlugin");
  });

Task("default")
  .IsDependentOn("copy-to-apphost");

RunTarget(target);