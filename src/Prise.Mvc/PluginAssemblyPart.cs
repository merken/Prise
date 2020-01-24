using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Prise.Mvc
{
    public class PluginAssemblyPart : AssemblyPart, ICompilationReferencesProvider
    {
        public PluginAssemblyPart(Assembly assembly) : base(assembly) { }

        // This solves the NullRef bug for in-memory assemblies from Prise
        IEnumerable<string> ICompilationReferencesProvider.GetReferencePaths() => Array.Empty<string>();
    }
}
