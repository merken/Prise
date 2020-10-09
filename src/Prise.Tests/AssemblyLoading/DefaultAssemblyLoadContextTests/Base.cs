using Prise.AssemblyLoading;
using Prise.Core;
using Prise.Tests.Plugins;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests.AssemblyLoading.DefaultAssemblyLoadContextTests
{
    public class Base : TestBase
    {
        protected T InvokeProtectedMethodOnLoadContextAndGetResult<T>(IAssemblyLoadContext loadContext, string methodName, params object[] args)
            where T : class
        {
            return loadContext
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(loadContext, args) as T;
        }

        protected IPluginLoadContext GetPluginLoadContext(string pluginAssemblyPath, Action<PluginLoadContext> configure = null)
        {
            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, GetContractType(), "netcoreapp3.1");
            configure?.Invoke(pluginLoadContext);
            return pluginLoadContext;
        }

        protected Type GetContractType()
        {
            return TestableTypeBuilder.NewTestableType()
               .WithName("IMyTestType")
               .WithNamespace("Test.Type")
               .Build();
        }

        protected TestContext<IAssemblyLoadContext> SetupAssemblyLoadContext()
        {
            var nativeUnloader = this.mockRepository.Create<INativeAssemblyUnloader>();
            var pluginDependencyResolver = this.mockRepository.Create<IPluginDependencyResolver>();
            var assemblyLoadStrategy = this.mockRepository.Create<IAssemblyLoadStrategy>();
            var pluginDependencyContext = this.mockRepository.Create<IPluginDependencyContext>();
            var resolver = this.mockRepository.Create<IAssemblyDependencyResolver>();
            var fileSystemUtility = this.mockRepository.Create<IFileSystemUtilities>();
            var runtimeDefaultAssemblyLoadContext = this.mockRepository.Create<IRuntimeDefaultAssemblyContext>();

            var loadContext = new DefaultAssemblyLoadContext(
               () => nativeUnloader.Object,
               () => pluginDependencyResolver.Object,
               () => assemblyLoadStrategy.Object,
               (c) => Task.FromResult(pluginDependencyContext.Object),
               (s) => resolver.Object,
               () => fileSystemUtility.Object,
               () => runtimeDefaultAssemblyLoadContext.Object);

            return new TestContext<IAssemblyLoadContext>(loadContext,
                nativeUnloader,
                pluginDependencyResolver,
                assemblyLoadStrategy,
                pluginDependencyContext,
                resolver,
                fileSystemUtility,
                runtimeDefaultAssemblyLoadContext
            );
        }

        protected string GetPathToAssemblies()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assemblies");
        }
    }
}