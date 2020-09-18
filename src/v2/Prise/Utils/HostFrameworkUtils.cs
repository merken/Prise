using System;
using System.Reflection;
using System.Runtime.Versioning;

namespace Prise.Utils
{
    public static class HostFrameworkUtils
    {
        public static string GetHostframeworkFromHost() =>
            Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

        public static string GetHostframeworkFromType(Type type) =>
            type.Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
    }
}