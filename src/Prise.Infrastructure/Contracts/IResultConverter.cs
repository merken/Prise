using System;

namespace Prise.Infrastructure
{
    public interface IResultConverter : IDisposable
    {
        object ConvertToLocalType(Type remoteType, object value);
    }
}