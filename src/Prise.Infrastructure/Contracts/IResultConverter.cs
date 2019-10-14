using System;

namespace Prise.Infrastructure
{
    public interface IResultConverter
    {
        object ConvertToLocalType(Type remoteType, object value);
    }
}