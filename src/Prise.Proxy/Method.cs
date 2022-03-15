using System;

namespace Prise.Proxy
{
    public struct Method
    {
        public string Name { get; }
        public Type? ReturnType { get; }

        public Method(string name, Type? returnType = null)
        {
            this.Name = name;
            this.ReturnType = returnType;
        }
    }
}