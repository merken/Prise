using System;

namespace Prise.Proxy
{
    public struct Parameter
    {
        public string Name { get; }
        public Type? Type { get; }

        public Parameter(string name, Type? type = null)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}