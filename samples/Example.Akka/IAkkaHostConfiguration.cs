using System;
using System.IO;
using System.Reflection;

namespace Example.Akka
{
    public interface IAkkaHostConfiguration
    {
        string GetPathToDist();
    }

    public class AkkaHostConfiguration : IAkkaHostConfiguration
    {
        public string GetPathToDist()
        {
             var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Plugins/dist"));
        }
    }
}