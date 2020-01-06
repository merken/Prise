var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var apikey = Argument("apikey", "");
var outputDir = "../dist";
var priseVersion = "1.4.5";
var pluginVersion = "1.4.5";
var assemblyDiscoveryVersion = "1.4.5";
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

    DotNetCoreBuild("Prise.AssemblyScanning.Discovery/Prise.AssemblyScanning.Discovery.csproj", netcoreapp2);
    DotNetCoreBuild("Prise.AssemblyScanning.Discovery/Prise.AssemblyScanning.Discovery.csproj", netcoreapp3);
    DotNetCoreBuild("Prise.Plugin/Prise.Plugin.csproj", netstandard2);
    DotNetCoreBuild("Prise.Plugin/Prise.Plugin.csproj", netstandard2_1);
    DotNetCoreBuild("Prise/Prise.csproj", netcoreapp2);
    DotNetCoreBuild("Prise/Prise.csproj", netcoreapp3);
});

private DotNetCorePackSettings GetPackSettings(string version){
  return new DotNetCorePackSettings {
        NoBuild = false,
        Configuration = configuration,
        OutputDirectory = outputDir,
        ArgumentCustomization = (args) => {
            args.Append("--include-source");

            return args
                .Append("/p:Version={0}", version)
                .Append("/p:AssemblyVersion={0}", version)
                .Append("/p:FileVersion={0}", version)
                .Append("/p:AssemblyInformationalVersion={0}", version);
        }
    };
}

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    // delete the dist folder
    CleanDirectories(outputDir);

    DotNetCorePack("Prise.AssemblyScanning.Discovery/Prise.AssemblyScanning.Discovery.csproj", GetPackSettings(assemblyDiscoveryVersion));
    DotNetCorePack("Prise.Plugin/Prise.Plugin.csproj", GetPackSettings(pluginVersion));
    DotNetCorePack("Prise/Prise.csproj", GetPackSettings(priseVersion));
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
    try
    {
      DotNetCoreNuGetPush(outputDir + "/Prise." + priseVersion +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
    try
    {
      DotNetCoreNuGetPush(outputDir + "/Prise.Plugin." + pluginVersion +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
    try
    {
      DotNetCoreNuGetPush(outputDir + "/Prise.AssemblyScanning.Discovery." + assemblyDiscoveryVersion +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);      
    }
  });

Task("default")
  .IsDependentOn("publish");

RunTarget(target);