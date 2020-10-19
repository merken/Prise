using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests
{
    public class MyServiceProxyParameterMismatch : MyServiceProxy
    {
        public MyServiceProxyParameterMismatch(object hostService) : base(hostService) { }
        public override Task<string> SetStringOverload(string value) => this.InvokeOnHostService<Task<string>>(); //Parameter mismatch

    }
    public class MyServiceProxy : Prise.Proxy.ReverseProxy, IMyService
    {
        public MyServiceProxy(object hostService) : base(hostService) { }

        public virtual decimal Add(decimal a, decimal b) => this.InvokeOnHostService<decimal>(a, b);
        public virtual decimal Add(decimal a, decimal b, decimal c) => this.InvokeOnHostService<decimal>(a, b, c);
        public virtual decimal Subtract(decimal a, decimal b) => this.InvokeOnHostService<decimal>(a, b);
        public virtual decimal Multiply(decimal a, decimal b) => this.InvokeOnHostService<decimal>(a, b);
        public virtual decimal Divide(decimal a, decimal b) => this.InvokeOnHostService<decimal>(a, b);
        public virtual Task<decimal> DivideAsync(decimal a, decimal b) => this.InvokeOnHostService<Task<decimal>>(a, b);
        public virtual Task<string> ReadFromDisk(string file) => this.InvokeOnHostService<Task<string>>(file);
        public virtual Task<string> ReadFromDisk(string file, string addition) => this.InvokeOnHostService<Task<string>>(file, addition);
        public virtual Task<string> GetString() => this.InvokeOnHostService<Task<string>>();
        public virtual void SetString(string value) => this.InvokeOnHostService(value);
        public virtual string ThrowsMyServiceException() => this.InvokeOnHostService<string>();
        public virtual Task<string> ThrowsMyServiceExceptionAsync() => this.InvokeOnHostService<Task<string>>();
        public virtual Task SetStringOverload() => this.InvokeOnHostService<Task>();
        public virtual Task<string> SetStringOverload(string value) => this.InvokeOnHostService<Task<string>>(value);
    }
}
