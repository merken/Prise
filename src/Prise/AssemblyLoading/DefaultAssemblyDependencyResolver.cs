using System.Reflection;
#if HAS_NATIVE_RESOLVER
using System.Runtime.Loader;
using System.IO;
#endif

namespace Prise.AssemblyLoading
{
    public class DefaultAssemblyDependencyResolver : IAssemblyDependencyResolver
    {
#if HAS_NATIVE_RESOLVER
        protected AssemblyDependencyResolver resolver;
#endif

        public DefaultAssemblyDependencyResolver(string fullPathToPluginAssembly)
        {
#if HAS_NATIVE_RESOLVER
            try
            {
                this.resolver = new AssemblyDependencyResolver(fullPathToPluginAssembly);
            }
            catch (System.ArgumentException ex)
            {
                throw new AssemblyLoadingException($"{nameof(AssemblyDependencyResolver)} could not be instantiated, possible issue with {fullPathToPluginAssembly} {Path.GetFileNameWithoutExtension(fullPathToPluginAssembly)}.deps.json file?", ex);
            }
#endif
        }

        public string ResolveAssemblyToPath(AssemblyName assemblyName)
        {
#if HAS_NATIVE_RESOLVER
            return this.resolver.ResolveAssemblyToPath(assemblyName);
#endif
            return null; // Not supported
        }

        public string ResolveUnmanagedDllToPath(string unmanagedDllName)
        {
#if HAS_NATIVE_RESOLVER

            return this.resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
#endif
            return null; // Not supported
        }

        public void Dispose()
        {
#if HAS_NATIVE_RESOLVER
            this.resolver= null;
#endif
        }
    }
}