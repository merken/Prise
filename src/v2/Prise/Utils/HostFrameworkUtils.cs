using System;
using System.Reflection;
using System.Runtime.Versioning;

namespace Prise.Utils
{
    public static class HostFrameworkUtils
    {
        public static string GetHostframeworkFromType(Type type) =>
            type.Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
    }
}