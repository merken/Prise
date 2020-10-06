using System;
using System.Runtime.CompilerServices;

namespace Prise.Utils
{
    public static class ValidationUtils
    {
        public static T ThrowIfNull<T>(this T obj, string name, [CallerMemberName] string constructor = "")
        where T : class
        {
            if (obj != null)
                return obj;

            throw new ArgumentNullException($"Parameter {typeof(T).Name} {name} is null for type {constructor}");
        }
    }
}