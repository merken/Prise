using System;

namespace Prise.Activation
{
    public class BootstrapperService
    {
        public string FieldName { get; set; }
        public Type ServiceType { get; set; }
        public Type ProxyType { get; set; }
    }
}