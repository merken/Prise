using System;

namespace Prise.Activation
{
    public interface IBootstrapperServiceProvider : IDisposable
    {
        object GetHostService(Type type);
    }
}