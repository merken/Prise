var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var apikey = Argument("apikey", "");
var outputDir = "../dist";
var semVer = "1.4.2";
var version = "1.4.2";
var infoVer = "1.4.2";
var nugetSource = "https://api.nuget.org/v3/index.json";

Task("build").Does( () =>
{ 
    var netcoreapp2 = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        Framework = "netcoreapp2.1"
    };
    var netcoreapp3 = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        Framework = "netcoreapp3.0"
    };

    var netstandard2 = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        Framework = "netstandard2.0"
    };
    var netstandard2_1 = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        Framework = "netstandard2.1"
    };

    DotNetCoreBuild("Prise.AssemblyScanning.Discovery/Prise.AssemblyScanning.Discovery.csproj", netstandard2);
    DotNetCoreBuild("Prise.Plugin/Prise.Plugin.csproj", netstandard2);
    DotNetCoreBuild("Prise.Plugin/Prise.Plugin.csproj", netstandard2_1);
    DotNetCoreBuild("Prise/Prise.csproj", netcoreapp2);
    DotNetCoreBuild("Prise/Prise.csproj", netcoreapp3);
});

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    // delete the dist folder
    CleanDirectories(outputDir);

    var settings = new DotNetCorePackSettings {
        NoBuild = false,
        Configuration = configuration,
        OutputDirectory = outputDir,
        ArgumentCustomization = (args) => {
            args.Append("--include-source");

            return args
                .Append("/p:Version={0}", semVer)
                .Append("/p:AssemblyVersion={0}", version)
                .Append("/p:FileVersion={0}", version)
                .Append("/p:AssemblyInformationalVersion={0}", infoVer);
        }
    };
    DotNetCorePack("Prise.AssemblyScanning.Discovery/Prise.AssemblyScanning.Discovery.csproj", settings);
    DotNetCorePack("Prise.Plugin/Prise.Plugin.csproj", settings);
    DotNetCorePack("Prise/Prise.csproj", settings);
  });


Task("push")
  .IsDependentOn("publish")
  .Does(() =>
  { 
    var settings = new DotNetCoreNuGetPushSettings 
    { 
        Source = nugetSource,
        ApiKey = apikey
    };

    DotNetCoreNuGetPush(outputDir + "/Prise." + version +  ".nupkg", settings); 
    DotNetCoreNuGetPush(outputDir + "/Prise.Plugin." + version +  ".nupkg", settings); 
    DotNetCoreNuGetPush(outputDir + "/Prise.AssemblyScanning.Discovery." + version +  ".nupkg", settings); 
  });

Task("default")
  .IsDependentOn("publish");

RunTarget(target);