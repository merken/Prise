var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var apikey = Argument("apikey", "");
var outputDir = "../_dist";
var betaVersion = "-beta1";
var priseVersion = "2.0.0";
var proxyVersion = "2.0.0";
var pluginVersion = "2.0.0";
var pluginReverseProxyVersion = "2.0.0";
var mvcVersion = "2.0.0";
var nugetSource = "https://api.nuget.org/v3/index.json";

Task("build").Does( () =>
{ 
    var net5 = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        Framework = "net5.0"
    };

    var netcoreapp2 = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        Framework = "netcoreapp2.1"
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

    DotNetCoreBuild("Prise.Plugin/Prise.Plugin.csproj", netstandard2);
    DotNetCoreBuild("Prise.Proxy/Prise.Proxy.csproj", netstandard2);
    DotNetCoreBuild("Prise.ReverseProxy/Prise.ReverseProxy.csproj", netstandard2);

    DotNetCoreBuild("Prise.Mvc/Prise.Mvc.csproj", netcoreapp2);
    DotNetCoreBuild("Prise.Mvc/Prise.Mvc.csproj", netcoreapp3_1);
    DotNetCoreBuild("Prise.Mvc/Prise.Mvc.csproj", net5);
    DotNetCoreBuild("Prise/Prise.csproj", netcoreapp2);
    DotNetCoreBuild("Prise/Prise.csproj", netcoreapp3_1);
    DotNetCoreBuild("Prise/Prise.csproj", net5);
});

private DotNetCorePackSettings GetPackSettings(string version, string betaVersion = null){  
  var versionString =version;
  if(betaVersion!=null)
    versionString +=betaVersion;
  var settings= new DotNetCorePackSettings {
        NoBuild = false,
        Configuration = configuration,
        OutputDirectory = outputDir,
        ArgumentCustomization = (args) => {
            return args
                .Append("--include-source")
                .Append("/p:Version={0}", versionString)
                .Append("/p:AssemblyVersion={0}", version)
                .Append("/p:FileVersion={0}", versionString)
                .Append("/p:AssemblyInformationalVersion={0}", version);
            ;
        }
    };
    if (betaVersion != null)
      settings.VersionSuffix = betaVersion;
    return settings;
}

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  {
    // delete the dist folder
    CleanDirectories(outputDir);
    DotNetCorePack("Prise.Mvc/Prise.Mvc.csproj", GetPackSettings(mvcVersion,  betaVersion));
    DotNetCorePack("Prise.Plugin/Prise.Plugin.csproj", GetPackSettings(pluginVersion, betaVersion));
    DotNetCorePack("Prise.ReverseProxy/Prise.ReverseProxy.csproj", GetPackSettings(pluginReverseProxyVersion, betaVersion));
    DotNetCorePack("Prise.Proxy/Prise.Proxy.csproj", GetPackSettings(proxyVersion, betaVersion));
    DotNetCorePack("Prise/Prise.csproj", GetPackSettings(priseVersion, betaVersion));
  });

private string GetPushVersion(string version, string betaVersion){
  return betaVersion !=null? version + betaVersion : version;
}
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
      DotNetCoreNuGetPush(outputDir + "/Prise.ReverseProxy." + GetPushVersion(pluginReverseProxyVersion, betaVersion) +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
    try
    {
      DotNetCoreNuGetPush(outputDir + "/Prise.Proxy." + GetPushVersion(proxyVersion,betaVersion) +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
    try
    {
      DotNetCoreNuGetPush(outputDir + "/Prise." + GetPushVersion(priseVersion,betaVersion) +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
    try
    {
      DotNetCoreNuGetPush(outputDir + "/Prise.Mvc." + GetPushVersion(mvcVersion,betaVersion) +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
    try
    {
      DotNetCoreNuGetPush(outputDir + "/Prise.Plugin." + GetPushVersion(pluginVersion,betaVersion) +  ".nupkg", settings); 
    }
    catch(Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
  });

Task("default")
  .IsDependentOn("publish");

RunTarget(target);