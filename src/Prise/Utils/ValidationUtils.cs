using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Prise.Utils
{
    public static class ValidationUtils
    {
        public static T ThrowIfNull<T>(this T obj, string name, [CallerFilePath] string filePath = "")
            where T : class
        {
            if (obj != null)
                return obj;

            throw new ArgumentNullException($"Parameter {typeof(T).Name} {name} is null for type {Path.GetFileNameWithoutExtension(Path.GetFileName(filePath))}");
        }

        public static string ThrowIfNullOrEmpty(this string str, string name, [CallerFilePath] string filePath = "")
        {
            if (!String.IsNullOrEmpty(str))
                return str;

            throw new ArgumentNullException($"Parameter string {name} is null or empty for type {Path.GetFileNameWithoutExtension(Path.GetFileName(filePath))}");
        }
    }
}