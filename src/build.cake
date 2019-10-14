var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var outputDir = "..\\dist";
var semVer = "0.0.1";
var version = "0.0.1";
var infoVer = "0.0.1";

Task("build").Does( () =>
{ 
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = "Release"
    };

    DotNetCoreBuild("Prise.Infrastructure\\Prise.Infrastructure.csproj", settings);
    DotNetCoreBuild("Prise.Infrastructure.NetCore\\Prise.Infrastructure.NetCore.csproj", settings);
});

Task("test").Does( () =>
{ 
  DotNetCoreTest("Tests\\Prise.Infrastructure.Tests\\Prise.Infrastructure.Tests.csproj");
  DotNetCoreTest("Tests\\Prise.Infrastructure.NetCore.Tests\\Prise.Infrastructure.NetCore.Tests.csproj");
}).IsDependentOn("build");

Task("publish")
  .IsDependentOn("build")
  .Does(() =>
  { 
    var settings = new DotNetCorePackSettings {
        NoBuild = true,
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
    DotNetCorePack("Prise.Infrastructure\\Prise.Infrastructure.csproj", settings);
    DotNetCorePack("Prise.Infrastructure.NetCore\\Prise.Infrastructure.NetCore.csproj", settings);
  });

Task("default")
  .IsDependentOn("build");

RunTarget(target);