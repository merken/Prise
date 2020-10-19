using System;
using System.Threading.Tasks;

namespace Prise.Tests
{
    [System.Serializable]
    public class MyServiceException : System.Exception
    {
        public MyServiceException() { }
        public MyServiceException(string message) : base(message) { }
        public MyServiceException(string message, System.Exception inner) : base(message, inner) { }
        protected MyServiceException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public interface IMyService
    {
        Task<string> GetString();
        void SetString(string value);
        decimal Add(decimal a, decimal b);
        decimal Add(decimal a, decimal b, decimal c);
        decimal Subtract(decimal a, decimal b);
        decimal Multiply(decimal a, decimal b);
        decimal Divide(decimal a, decimal b);
        Task<decimal> DivideAsync(decimal a, decimal b);
        Task<string> ReadFromDisk(string file);
        Task<string> ReadFromDisk(string file, string addition);
        string ThrowsMyServiceException();
        Task<string> ThrowsMyServiceExceptionAsync();
        Task SetStringOverload();
        Task<string> SetStringOverload(string value);
    }
}
