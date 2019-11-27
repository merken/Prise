using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Prise
{
    public class RuntimePlatformContext : IRuntimePlatformContext
    {
        public IEnumerable<string> GetPlatformExtensions() => GetPlatformDependencyFileExtensions();

        public IEnumerable<string> GetPluginDependencyNames(string nameWithoutFileExtension) =>
            GetPluginDependencyFileExtensions()
                .Select(ext => $"{nameWithoutFileExtension}{ext}");

        public IEnumerable<string> GetPlatformDependencyNames(string nameWithoutFileExtension) =>
             GetPlatformDependencyFileCandidates(nameWithoutFileExtension);

        public Prise.Infrastructure.RuntimeInformation GetRuntimeInformation()
        {
            var runtimeBasePath = String.Empty;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                runtimeBasePath = "C:\\Program Files\\dotnet\\shared";
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                runtimeBasePath = "/usr/share/dotnet/shared";

            var platformIndependendPath = System.IO.Path.GetFullPath(runtimeBasePath);
            var runtimes = new List<Runtime>();
            foreach (var pathToDirectory in System.IO.Directory.GetDirectories(platformIndependendPath))
            {
                var runtimeName = System.IO.Path.GetFileName(pathToDirectory); // Gets the directory name
                var runtimeType = ParseType(runtimeName);
                foreach (var pathToVersion in System.IO.Directory.GetDirectories(pathToDirectory))
                {
                    var runtimeVersion = System.IO.Path.GetFileName(pathToVersion); // Gets the directory name
                    var runtimeLocation = pathToVersion;
                    runtimes.Add(new Runtime
                    {
                        Version = runtimeVersion,
                        Location = runtimeLocation,
                        RuntimeType = runtimeType
                    });
                }
            }

            return new Prise.Infrastructure.RuntimeInformation
            {
                Runtimes = runtimes
            };
        }

        private RuntimeType ParseType(string runtimeName)
        {
            switch (runtimeName.ToUpper())
            {
                case "MICROSOFT.ASPNETCORE.ALL": return RuntimeType.AspNetCoreAll;
                case "MICROSOFT.ASPNETCORE.APP": return RuntimeType.AspNetCoreApp;
                case "MICROSOFT.NETCORE.APP": return RuntimeType.NetCoreApp;
                case "MICROSOFT.WINDOWSDESKTOP.APP": return RuntimeType.WindowsDesktopApp;
            }
            throw new PrisePluginException($"Runtime {runtimeName} could not be parsed");
        }

        private string[] GetPluginDependencyFileExtensions()
        {
            return new[]
            {
                ".dll",
                ".ni.dll",
                ".exe",
                ".ni.exe"
            };
        }

        private string[] GetPlatformDependencyFileCandidates(string fileNameWithoutExtension)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new[] { $"{fileNameWithoutExtension}.dll" };
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new[] { $"{fileNameWithoutExtension}.so", $"{fileNameWithoutExtension}.so.1", $"lib{fileNameWithoutExtension}.so", $"lib{fileNameWithoutExtension}.so.1" };
            //if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            throw new PrisePluginException("Platform is not supported");
        }

        private string[] GetPlatformDependencyFileExtensions()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new[] { ".dll" };
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new[] { ".so", ".so.1" };
            //if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            throw new PrisePluginException("Platform is not supported");
        }
    }
}
