using System;

namespace Prise.Proxy
{
    [Flags]
    public enum MethodFindingStrategy
    {
        MethodNameMustMatch,
        MethodReturnTypeMustMatch,
        ParameterCountMustMatch,
        ParameterTypeMustMatch
    }
}