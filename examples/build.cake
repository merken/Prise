var target = Argument("target", "default");
var configuration = Argument("configuration", "Debug");

private string[] GetAllExampleProjects(){
    return GetFiles("./**/*.csproj").Select(d=>d.FullPath).ToArray();
}

Task("clean").Does( () =>
{ 
    foreach(var project in GetAllExampleProjects())
    {
       var cleanSettings = new DotNetCoreCleanSettings
        {
            Configuration = configuration
        };
        DotNetCoreClean(project, cleanSettings);
    }
});

Task("build")
  .IsDependentOn("clean")
  .Does( () =>
{ 
    foreach(var project in GetAllExampleProjects())
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = configuration
        };

        DotNetCoreBuild(project, settings);
    }
});

Task("default")
  .IsDependentOn("build");

RunTarget(target);