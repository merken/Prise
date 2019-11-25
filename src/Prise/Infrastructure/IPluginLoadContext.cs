using Prise.AssemblyScanning;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prise.Infrastructure
{
    public interface IPluginLoadContext
    {
        string PluginAssemblyName { get; }
        string PluginAssemblyPath { get; }
    }

    public class PluginLoadContext<T> : IPluginLoadContext
    {
        public string PluginAssemblyName { get; private set; }

        public string PluginAssemblyPath { get; private set; }

        public PluginLoadContext(string assemblyName, string assemblyPath)
        {
            this.PluginAssemblyName = assemblyName;
            this.PluginAssemblyPath = assemblyPath;
        }

        public static IPluginLoadContext FromAssemblyScanResult(AssemblyScanResult<T> assemblyScanResult)
            => new PluginLoadContext<T>(assemblyScanResult.AssemblyName, assemblyScanResult.AssemblyPath);
    }
}
