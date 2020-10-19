using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests
{
    public class MyService : IMyService
    {
        private string value = "Test";
        public decimal Add(decimal a, decimal b) => a + b;
        public decimal Add(decimal a, decimal b, decimal c) => a + b + c;
        public decimal Subtract(decimal a, decimal b) => a - b;
        public decimal Multiply(decimal a, decimal b) => a * b;
        public decimal Divide(decimal a, decimal b) => a / b;
        public async Task<decimal> DivideAsync(decimal a, decimal b)
        {
            await Task.Delay(100);
            return a / b;
        }
        public Task<string> ReadFromDisk(string file) => File.ReadAllTextAsync(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), file));
        public async Task<string> ReadFromDisk(string file, string addition)
        {
            var content = await File.ReadAllTextAsync(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), file));
            return content + addition;
        }
        public Task<string> GetString() => Task.FromResult(this.value);
        public void SetString(string value)
        {
            this.value = value;
        }
        public string ThrowsMyServiceException() => throw new MyServiceException(value);
        public Task<string> ThrowsMyServiceExceptionAsync()
        {
            return Task.FromException<string>(new MyServiceException(value));
        }

        public async Task SetStringOverload()
        {
            await Task.Delay(100);
            this.value = $"{this.value} {this.value}";
        }

        public async Task<string> SetStringOverload(string value)
        {
            await Task.Delay(100);
            this.value = value;
            return this.value;
        }
    }
}
