using System;

namespace Prise.Util
{
    public static class ValidationExtensions
    {
        public static T ThrowIfNull<T>(this T obj, string member)
        {
            if (obj == null)
                throw new ArgumentNullException(member);
            return obj;
        }

        public static string ThrowIfNullOrEmpty(this string obj, string member)
        {
            if (String.IsNullOrEmpty(obj))
                throw new ArgumentNullException(member);
            return obj;
        }

        public static string ThrowIfNullOrWhiteSpace(this string obj, string member)
        {
            if (String.IsNullOrWhiteSpace(obj))
                throw new ArgumentNullException(member);
            return obj;
        }
    }
}
