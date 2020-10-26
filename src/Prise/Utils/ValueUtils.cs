namespace Prise.Utils
{
    public static class ValueUtils
    {
        public static string ValueOrDefault(this string value, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;
            return value;
        }
    }
}