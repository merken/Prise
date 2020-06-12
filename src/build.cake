var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var apikey = Argument("apikey", "");
var outputDir = "../dist";
var betaVersion = "1.9.0-beta1";
var priseVersion = "1.7.5";
var proxyVersion = "1.7.5";
var pluginVersion = "1.7.0";
var pluginBridgeVersion = "1.7.5";
var mvcVersion = "1.7.5";
var assemblyDiscoveryVersion = "1.7.1";
var nugetDiscoveryVersion = "1.7.4";
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

    var netcoreapp3_1 = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        Framework = "netcoreapp3.1"
    };

    var netstandard2 = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        Framework = "netstandard2.0"
    };

    DotNetCoreBuild("Prise.AssemblyScanning.Discovery.Nuget/Prise.AssemblyScanning.Discovery.Nuget.csproj", netstandard2);
    DotNetCoreBuild("Prise.AssemblyScanning.Discovery/Prise.AssemblyScanning.Discovery.csproj", netstandard2);
   
    DotNetCoreBuild("Prise.Plugin/Prise.Plugin.csproj", netstandard2);
    DotNetCoreBuild("Prise.Proxy/Prise.Proxy.csproj", netstandard2);
    DotNetCoreBuild("Prise.PluginBridge/Prise.PluginBridge.csproj", netstandard2);

    DotNetCoreBuild("Prise.MVC/Prise.MVC.csproj", netcoreapp2);
    DotNetCoreBuild("Prise.MVC/Prise.MVC.csproj", netcoreapp3);
    DotNetCoreBuild("Prise.MVC/Prise.MVC.csproj", netcoreapp3_1);
    DotNetCoreBuild("Prise/Prise.csproj", netcoreapp2);
    DotNetCoreBuild("Prise/Prise.csproj", netcoreapp3);
    DotNetCoreBuild("Prise/Prise.csproj", netcoreapp3_1);
});

private DotNetCorePackSettings GetPackSettings(string version, string betaVersion = null){
  if (betaVersion == null)
    betaVersion = version;
  
  return new DotNetCorePackSettings {
        NoBuild = false,
        Configuration = configuration,
        OutputDirectory = outputDir,
        ArgumentCustomization = (args) => {
            return args
                .Append("--include-source")
                .Append("/p:Version={0}", betaVersion)
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

    DotNetCorePack("Prise.AssemblyScanning.Discovery.Nuget/Prise.AssemblyScanning.Discovery.Nuget.csproj", GetPackSettings(nugetDiscoveryVersion));
    DotNetCorePack("Prise.AssemblyScanning.Discovery/Prise.AssemblyScanning.Discovery.csproj", GetPackSettings(assemblyDiscoveryVersion));
    DotNetCorePack("Prise.MVC/Prise.MVC.csproj", GetPackSettings(mvcVersion));
    DotNetCorePack("Prise.Plugin/Prise.Plugin.csproj", GetPackSettings(pluginVersion));
    DotNetCorePack("Prise.PluginBridge/Prise.PluginBridge.csproj", GetPackSettings(pluginBridgeVersion));
    DotNetCorePack("Prise.Proxy/Prise.Proxy.csproj", GetPackSettings(proxyVersion));
    DotNetCorePack("Prise/Prise.csproj", GetPackSettings(priseVersion));
  });

Task("beta")
  .IsDependentOn("build")
  .Does(() =>
  {
    // delete the dist folder
    CleanDirectories(outputDir);
    Console.WriteLine("Creating beta versions " + betaVersion);
    DotNetCorePack("Prise.AssemblyScanning.Discovery.Nuget/Prise.AssemblyScanning.Discovery.Nuget.csproj", GetPackSettings(nugetDiscoveryVersion, betaVersion));
    DotNetCorePack("Prise.AssemblyScanning.Discovery/Prise.AssemblyScanning.Discovery.csproj", GetPackSettings(assemblyDiscoveryVersion, betaVersion));
    DotNetCorePack("Prise.MVC/Prise.MVC.csproj", GetPackSettings(mvcVersion,  betaVersion));
    DotNetCorePack("Prise.Plugin/Prise.Plugin.csproj", GetPackSettings(pluginVersion, betaVersion));
    DotNetCorePack("Prise.PluginBridge/Prise.PluginBridge.csproj", GetPackSettings(pluginBridgeVersion, betaVersion));
    DotNetCorePack("Prise.Proxy/Prise.Proxy.csproj", GetPackSettings(proxyVersion, betaVersion));
    DotNetCorePack("Prise/Prise.csproj", GetPackSettings(priseVersion, betaVersion));
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
      DotNetCoreNuGetPush(outputDir + "/Prise.PluginBridge." + pluginBridgeVersion +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
    try
    {
      DotNetCoreNuGetPush(outputDir + "/Prise.Proxy." + proxyVersion +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
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
      DotNetCoreNuGetPush(outputDir + "/Prise.MVC." + mvcVersion +  ".nupkg", settings); 
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
    try
    {
      DotNetCoreNuGetPush(outputDir + "/Prise.AssemblyScanning.Discovery.Nuget." + nugetDiscoveryVersion +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);      
    }
  });

Task("default")
  .IsDependentOn("publish");

RunTarget(target);