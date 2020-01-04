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
  CleanProject("ProductsReaderPlugin");
  CleanProject("ProductsWriterPlugin");
  CleanProject("ProductsDeleterPlugin");
});

Task("build")
  .IsDependentOn("clean")
  .Does( () =>
{ 
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration
    };

    DotNetCoreBuild("Plugins/ProductsReaderPlugin/ProductsReaderPlugin.csproj", settings);
    DotNetCoreBuild("Plugins/ProductsWriterPlugin/ProductsWriterPlugin.csproj", settings);
    DotNetCoreBuild("Plugins/ProductsDeleterPlugin/ProductsDeleterPlugin.csproj", settings);
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    DotNetCorePublish("Plugins/ProductsReaderPlugin/ProductsReaderPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/ProductsReaderPlugin"
    });

    DotNetCorePublish("Plugins/ProductsWriterPlugin/ProductsWriterPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/ProductsWriterPlugin"
    });

    DotNetCorePublish("Plugins/ProductsDeleterPlugin/ProductsDeleterPlugin.csproj", new DotNetCorePublishSettings
    {
        NoBuild = true,
        Configuration = configuration,
        OutputDirectory = "publish/ProductsDeleterPlugin"
    });
  });

Task("copy-to-apphost")
  .IsDependentOn("publish")
  .Does(() =>
  {
    CopyDirectory("publish/ProductsReaderPlugin", "MyHost/bin/debug/netcoreapp3.0/Plugins/ProductsReaderPlugin");
    CopyDirectory("publish/ProductsWriterPlugin", "MyHost/bin/debug/netcoreapp3.0/Plugins/ProductsWriterPlugin");
    CopyDirectory("publish/ProductsDeleterPlugin", "MyHost/bin/debug/netcoreapp3.0/Plugins/ProductsDeleterPlugin");
  });

Task("default")
  .IsDependentOn("copy-to-apphost");

RunTarget(target);