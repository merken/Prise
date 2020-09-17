using System;

namespace Prise.Core
{
    public class PluginPlatformVersion
    {
        public string Version { get; private set; }
        public RuntimeType Runtime { get; private set; }
        public bool IsSpecified => !String.IsNullOrEmpty(Version);

        public static PluginPlatformVersion Create(string version, RuntimeType runtime = RuntimeType.UnSpecified) => new PluginPlatformVersion
        {
            Version = version,
            Runtime = runtime
        };

        public static PluginPlatformVersion Empty() => new PluginPlatformVersion();
    }
}