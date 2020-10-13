using System;
using System.Collections.Generic;
using System.Linq;
using Prise.Core;
using Prise.Utils;

namespace Prise.Platform
{
    public class DefaultRuntimePlatformContext : IRuntimePlatformContext
    {
        private readonly IPlatformAbstraction platformAbstraction;
        private readonly IDirectoryTraverser directoryTraverser;

        public DefaultRuntimePlatformContext(
            Func<IPlatformAbstraction> platformAbstractionFactory,
            Func<IDirectoryTraverser> directoryTraverserFactory)
        {
            this.platformAbstraction = platformAbstractionFactory.ThrowIfNull(nameof(platformAbstractionFactory))();
            this.directoryTraverser = directoryTraverserFactory.ThrowIfNull(nameof(directoryTraverserFactory))();
        }

        public IEnumerable<string> GetPlatformExtensions() => GetPlatformDependencyFileExtensions();

        public IEnumerable<string> GetPluginDependencyNames(string nameWithoutFileExtension) =>
            GetPluginDependencyFileExtensions()
                .Select(ext => $"{nameWithoutFileExtension}{ext}");

        public IEnumerable<string> GetPlatformDependencyNames(string nameWithoutFileExtension) =>
             GetPlatformDependencyFileCandidates(nameWithoutFileExtension);

        public RuntimeInfo GetRuntimeInfo()
        {
            var runtimeBasePath = GetRuntimeBasePath();

            var platformIndependendPath = System.IO.Path.GetFullPath(runtimeBasePath);
            var runtimes = new List<Runtime>();
            foreach (var pathToDirectory in this.directoryTraverser.TraverseDirectories(platformIndependendPath))
            {
                var runtimeName = System.IO.Path.GetFileName(pathToDirectory); // Gets the directory name
                var runtimeType = ParseType(runtimeName);
                foreach (var pathToVersion in this.directoryTraverser.TraverseDirectories(pathToDirectory))
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

            return new RuntimeInfo
            {
                Runtimes = runtimes
            };
        }

        private string GetRuntimeBasePath()
        {
            if (this.platformAbstraction.IsWindows())
                return "C:\\Program Files\\dotnet\\shared";
            if (this.platformAbstraction.IsLinux())
                return "/usr/share/dotnet/shared";
            if (this.platformAbstraction.IsOSX())
                return "/usr/local/share/dotnet/shared";
            throw new PlatformException($"Platform {System.Runtime.InteropServices.RuntimeInformation.OSDescription} is not supported");
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
            throw new PlatformException($"Runtime {runtimeName} could not be parsed");
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
            if (this.platformAbstraction.IsWindows())
                return new[] { $"{fileNameWithoutExtension}.dll" };
            if (this.platformAbstraction.IsLinux())
                return new[] {
                    $"{fileNameWithoutExtension}.so",
                    $"{fileNameWithoutExtension}.so.1",
                    $"lib{fileNameWithoutExtension}.so",
                    $"lib{fileNameWithoutExtension}.so.1" };
            if (this.platformAbstraction.IsOSX())
                return new[] {
                    $"{fileNameWithoutExtension}.dylib",
                    $"lib{fileNameWithoutExtension}.dylib" };

            throw new PlatformException($"Platform {System.Runtime.InteropServices.RuntimeInformation.OSDescription} is not supported");
        }

        private string[] GetPlatformDependencyFileExtensions()
        {
            if (this.platformAbstraction.IsWindows())
                return new[] { ".dll" };
            if (this.platformAbstraction.IsLinux())
                return new[] { ".so", ".so.1" };
            if (this.platformAbstraction.IsOSX())
                return new[] { ".dylib" };

            throw new PlatformException($"Platform {System.Runtime.InteropServices.RuntimeInformation.OSDescription} is not supported");
        }
    }
}