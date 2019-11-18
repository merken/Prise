using System.Diagnostics;

namespace Prise
{
    [DebuggerDisplay("{CanProceed} - {Value}")]
    public class ValueOrProceed<T>
    {
        public static ValueOrProceed<T> Proceed() => new ValueOrProceed<T>
        {
            CanProceed = true
        };

        public static ValueOrProceed<T> FromValue(T value, bool proceed) => new ValueOrProceed<T>
        {
            Value = value,
            CanProceed = proceed
        };

        public T Value { get; private set; }
        public bool CanProceed { get; private set; }
    }
}
