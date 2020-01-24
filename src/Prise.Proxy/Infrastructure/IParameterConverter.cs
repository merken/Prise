using System;

namespace Prise.Proxy
{
    public interface IParameterConverter : IDisposable
    {
        object ConvertToRemoteType(Type localType, object value);
    }
}
