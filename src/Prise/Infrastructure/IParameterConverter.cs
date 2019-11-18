using System;

namespace Prise.Infrastructure
{
    public interface IParameterConverter : IDisposable
    {
        object ConvertToRemoteType(Type localType, object value);
    }
}