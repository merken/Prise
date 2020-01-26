var target = Argument("target", "default");
var configuration = Argument("configuration", "Debug");
var plugins = new[] { "PluginA", "PluginB", "PluginC", "PluginCFromNetwork" };
var defaultPlugins = new[] { "PluginA", "PluginB", "PluginC" };
var multiPlatformPlugins = new[] { "PluginD", "PluginE" };
var networkPlugins = new[] { "PluginCFromNetwork" };

private void CleanProject(string projectDirectory){
    var projectFile = $"IntegrationTestsPlugins/{projectDirectory}/{projectDirectory}.csproj";
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
    CleanProject(plugin);
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

    foreach (var plugin in plugins)
    {
      DotNetCoreBuild($"IntegrationTestsPlugins/{plugin}/{plugin}.csproj", settings);
    }
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    foreach (var plugin in plugins)
    {
      DotNetCorePublish($"IntegrationTestsPlugins/{plugin}/{plugin}.csproj", new DotNetCorePublishSettings
      {
          NoBuild = true,
          Configuration = configuration,
          OutputDirectory = $"publish/{plugin}"
      });
    }
    foreach (var plugin in multiPlatformPlugins)
    {
      DotNetCorePublish($"IntegrationTestsPlugins/{plugin}/{plugin}.net2.csproj", new DotNetCorePublishSettings
      {
          NoBuild = false,
          Configuration = configuration,
          Framework = "netcoreapp2.1",
          OutputDirectory = $"publish/netcoreapp2.1/{plugin}"
      });
      DotNetCorePublish($"IntegrationTestsPlugins/{plugin}/{plugin}.csproj", new DotNetCorePublishSettings
      {
          NoBuild = false,
          Configuration = configuration,
          Framework = "netcoreapp3.0",
          OutputDirectory = $"publish/netcoreapp3.0/{plugin}"
      });
    }
  });

Task("copy-to-testhost")
  .IsDependentOn("publish")
  .Does(() =>
  {
    foreach (var plugin in defaultPlugins)
    {
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp2.1/Plugins/{plugin}");
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp2.1/Plugins/{plugin}");
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp3.0/Plugins/{plugin}");
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.0/Plugins/{plugin}");
    }
    foreach (var plugin in multiPlatformPlugins)
    {
      CopyDirectory($"publish/netcoreapp2.1/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp2.1/Plugins/{plugin}");
      CopyDirectory($"publish/netcoreapp2.1/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp2.1/Plugins/{plugin}");
      CopyDirectory($"publish/netcoreapp3.0/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp3.0/Plugins/{plugin}");
      CopyDirectory($"publish/netcoreapp3.0/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.0/Plugins/{plugin}");
    }

    foreach (var plugin in networkPlugins)
    {
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTestsHost/Plugins/{plugin}");
    }
  });

Task("default")
  .IsDependentOn("copy-to-testhost");

RunTarget(target);