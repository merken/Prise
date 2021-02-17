using System;
using System.Collections.Generic;

namespace Contract
{
    [Serializable]
    public class ComponentInput
    {
        public string Text { get; set; }
        public List<ComponentInput> Children { get; set; }
    }
}