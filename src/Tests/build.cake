var target = Argument("target", "default");
var configuration = Argument("configuration", "Debug");
var plugins = new[] { "PluginA", "PluginB", "PluginC", "PluginCFromNetwork", "LegacyPlugin1.4", "LegacyPlugin1.5" };
var defaultPlugins = new[] { "PluginA", "PluginB", "PluginC" };
var multiPlatformPlugins = new[] { "PluginD", "PluginE", "PluginF", "PluginG" };
var nugetPlugins = new[] { "PluginG" };
var legacyPlugins = new[] { "LegacyPlugin1.4", "LegacyPlugin1.5" };
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

    foreach (var plugin in multiPlatformPlugins)
    {
      DotNetCoreBuild($"IntegrationTestsPlugins/{plugin}/{plugin}.net3.csproj", settings);
    }

    foreach (var plugin in multiPlatformPlugins)
    {
      DotNetCoreBuild($"IntegrationTestsPlugins/{plugin}/{plugin}.net2.csproj", settings);
    }
});

private void FixAssemblyIssues(string pathToDlls)
{
  using(var process = StartAndReturnProcess("dotnet", new ProcessSettings{ Arguments = $"FileBulkDateChanger/FileBulkDateChanger.dll {pathToDlls}" }))
  {
      process.WaitForExit(2000);//Waits for 2 seconds
  }
}

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
    foreach (var plugin in multiPlatformPlugins.Except(nugetPlugins))
    {
      DotNetCorePublish($"IntegrationTestsPlugins/{plugin}/{plugin}.net2.csproj", new DotNetCorePublishSettings
      {
          NoBuild = false,
          Configuration = configuration,
          Framework = "netcoreapp2.1",
          OutputDirectory = $"publish/netcoreapp2.1/{plugin}"
      });

      DotNetCorePublish($"IntegrationTestsPlugins/{plugin}/{plugin}.net3.csproj", new DotNetCorePublishSettings
      {
          NoBuild = false,
          Configuration = configuration,
          Framework = "netcoreapp3.0",
          OutputDirectory = $"publish/netcoreapp3.0/{plugin}"
      });

      DotNetCorePublish($"IntegrationTestsPlugins/{plugin}/{plugin}.csproj", new DotNetCorePublishSettings
      {
          NoBuild = false,
          Configuration = configuration,
          Framework = "netcoreapp3.1",
          OutputDirectory = $"publish/netcoreapp3.1/{plugin}"
      });
    }

    foreach (var plugin in nugetPlugins)
    {
      DotNetCorePublish($"IntegrationTestsPlugins/{plugin}/{plugin}.net2.csproj", new DotNetCorePublishSettings
      {
          NoBuild = false,
          Configuration = configuration,
          Framework = "netcoreapp2.1"
      });
      FixAssemblyIssues($"IntegrationTestsPlugins/{plugin}/bin/{configuration}/netcoreapp2.1/publish");
      DotNetCorePack($"IntegrationTestsPlugins/{plugin}/{plugin}.net2.csproj", new DotNetCorePackSettings
      {
          NoBuild = true,
          Configuration = configuration,
          OutputDirectory = $"publish/netcoreapp2.1",
          ArgumentCustomization = (args) => args.Append("/p:nuspecfile=PluginG.net2.nuspec")
      });

      DotNetCorePublish($"IntegrationTestsPlugins/{plugin}/{plugin}.net3.csproj", new DotNetCorePublishSettings
      {
          NoBuild = false,
          Configuration = configuration,
          Framework = "netcoreapp3.0"
      });
      FixAssemblyIssues($"IntegrationTestsPlugins/{plugin}/bin/{configuration}/netcoreapp3.0/publish");
      DotNetCorePack($"IntegrationTestsPlugins/{plugin}/{plugin}.net3.csproj", new DotNetCorePackSettings
      {
          NoBuild = true,
          Configuration = configuration,
          OutputDirectory = $"publish/netcoreapp3.0",
          ArgumentCustomization = (args) => args.Append("/p:nuspecfile=PluginG.net3.nuspec")
      });

      DotNetCorePublish($"IntegrationTestsPlugins/{plugin}/{plugin}.csproj", new DotNetCorePublishSettings
      {
          NoBuild = false,
          Configuration = configuration,
          Framework = "netcoreapp3.1"
      });
      FixAssemblyIssues($"IntegrationTestsPlugins/{plugin}/bin/{configuration}/netcoreapp3.1/publish");
      DotNetCorePack($"IntegrationTestsPlugins/{plugin}/{plugin}.csproj", new DotNetCorePackSettings
      {
          NoBuild = true,
          Configuration = configuration,
          OutputDirectory = $"publish/netcoreapp3.1",
          ArgumentCustomization = (args) => args.Append("/p:nuspecfile=PluginG.nuspec")
      });
    }
  });

Task("copy-to-testhost")
  .IsDependentOn("publish")
  .Does(() =>
  {
    foreach (var plugin in defaultPlugins.Union(legacyPlugins))
    {
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp2.1/Plugins/{plugin}");
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp2.1/Plugins/{plugin}");
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp3.0/Plugins/{plugin}");
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.0/Plugins/{plugin}");
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp3.1/Plugins/{plugin}");
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.1/Plugins/{plugin}");
    }

    foreach (var plugin in multiPlatformPlugins.Except(nugetPlugins))
    {
      CopyDirectory($"publish/netcoreapp2.1/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp2.1/Plugins/{plugin}");
      CopyDirectory($"publish/netcoreapp2.1/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp2.1/Plugins/{plugin}");
      CopyDirectory($"publish/netcoreapp3.0/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp3.0/Plugins/{plugin}");
      CopyDirectory($"publish/netcoreapp3.0/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.0/Plugins/{plugin}");
      CopyDirectory($"publish/netcoreapp3.1/{plugin}", $"Prise.IntegrationTests/bin/debug/netcoreapp3.1/Plugins/{plugin}");
      CopyDirectory($"publish/netcoreapp3.1/{plugin}", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.1/Plugins/{plugin}");
    }

    foreach (var plugin in nugetPlugins)
    {
      CopyFiles($"publish/netcoreapp2.1/*.nupkg", $"Prise.IntegrationTests/bin/debug/netcoreapp2.1/Plugins");
      CopyFiles($"publish/netcoreapp2.1/*.nupkg", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp2.1/Plugins");
      CopyFiles($"publish/netcoreapp3.0/*.nupkg", $"Prise.IntegrationTests/bin/debug/netcoreapp3.0/Plugins");
      CopyFiles($"publish/netcoreapp3.0/*.nupkg", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.0/Plugins");
      CopyFiles($"publish/netcoreapp3.1/*.nupkg", $"Prise.IntegrationTests/bin/debug/netcoreapp3.1/Plugins");
      CopyFiles($"publish/netcoreapp3.1/*.nupkg", $"Prise.IntegrationTestsHost/bin/debug/netcoreapp3.1/Plugins");
    }

    foreach (var plugin in networkPlugins)
    {
      CopyDirectory($"publish/{plugin}", $"Prise.IntegrationTestsHost/Plugins/{plugin}");
    }
  });

Task("default")
  .IsDependentOn("copy-to-testhost");

RunTarget(target);