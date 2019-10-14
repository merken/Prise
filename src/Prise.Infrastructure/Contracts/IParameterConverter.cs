using System;

namespace Prise.Infrastructure
{
    public interface IParameterConverter
    {
        object ConvertToRemoteType(Type localType, object value);
    }
}