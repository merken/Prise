var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var outputDir = "..\\dist";
var semVer = "1.2.3";
var version = "1.2.3";
var infoVer = "1.2.3";

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

    DotNetCoreBuild("Prise.Infrastructure\\Prise.Infrastructure.csproj", netstandard2);
    DotNetCoreBuild("Prise.Infrastructure\\Prise.Infrastructure.csproj", netstandard2_1);
    DotNetCoreBuild("Prise.Infrastructure.NetCore\\Prise.Infrastructure.NetCore.csproj", netcoreapp2);
    DotNetCoreBuild("Prise.Infrastructure.NetCore\\Prise.Infrastructure.NetCore.csproj", netcoreapp3);
});

Task("test").Does( () =>
{ 
  var netcoreapp2 = new DotNetCoreTestSettings
  {
      Configuration = "Release",
      Framework = "netcoreapp2.1"
  };
  var netcoreapp3 = new DotNetCoreTestSettings
  {
      Configuration = "Release",
      Framework = "netcoreapp3.0"
  };

  var netstandard2 = new DotNetCoreTestSettings
  {
      Configuration = "Release",
      Framework = "netstandard2.0"
  };
  var netstandard2_1 = new DotNetCoreTestSettings
  {
      Configuration = "Release",
      Framework = "netstandard2.1"
  };

  DotNetCoreTest("Tests\\Prise.Infrastructure.Tests\\Prise.Infrastructure.Tests.csproj", netstandard2);
  DotNetCoreTest("Tests\\Prise.Infrastructure.Tests\\Prise.Infrastructure.Tests.csproj", netstandard2_1);
  DotNetCoreTest("Tests\\Prise.Infrastructure.NetCore.Tests\\Prise.Infrastructure.NetCore.Tests.csproj",netcoreapp2);
  DotNetCoreTest("Tests\\Prise.Infrastructure.NetCore.Tests\\Prise.Infrastructure.NetCore.Tests.csproj",netcoreapp3);
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
  .IsDependentOn("publish");

RunTarget(target);