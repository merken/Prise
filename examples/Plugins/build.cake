var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var outputDir = "..\\dist";

Task("build").Does( () =>
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
        Configuration = configuration,
        OutputDirectory = "publish/PluginA"
    });

    DotNetCorePublish("PluginB\\PluginB.csproj", new DotNetCorePublishSettings
    {
        Configuration = configuration,
        OutputDirectory = "publish/PluginB"
    });
    DotNetCorePublish("PluginC\\PluginC.csproj", new DotNetCorePublishSettings
    {
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
  .IsDependentOn("build");

RunTarget(target);