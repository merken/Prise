using System;
using System.Collections.Generic;
using System.Reflection;

namespace Prise.AssemblyLoading
{
    public class RuntimeAssemblyShim : IAssemblyShim
    {
        private readonly Assembly assembly;
        private readonly RuntimeLoadFlag flag;
        public RuntimeAssemblyShim(Assembly assembly, RuntimeLoadFlag flag)
        {
            this.assembly = assembly;
            this.flag = flag;
        }

        public Assembly Assembly => assembly;
        public IEnumerable<Type> Types => assembly.GetTypes();
        public RuntimeLoadFlag RuntimeLoadFlag => flag;
    }
}