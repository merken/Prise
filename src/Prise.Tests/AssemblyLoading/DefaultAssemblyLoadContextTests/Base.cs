using Moq;
using Prise.AssemblyLoading;
using Prise.Core;
using Prise.Tests.Plugins;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Prise.Tests.AssemblyLoading.DefaultAssemblyLoadContextTests
{
    public class AssemblyLoadContextWithLoadedPluginTestContext
    {
        public string PluginAssemblyPath { get; set; }
        public string InitialPluginLoadDirectory { get; set; }
        public string NewtonsoftAssemblyPath { get; set; }
        public AssemblyName NewtonsoftAssemblyName { get; set; }
        public Assembly NewtonsoftAssembly { get; set; }
        public IPluginLoadContext PluginLoadContext { get; set; }
        public TestContext<IAssemblyLoadContext> TestContext { get; set; }

        public IAssemblyLoadContext Sut() => this.TestContext.Sut();

        public Mock<M> GetMock<M>()
            where M : class
        {
            return this.TestContext.GetMock<M>();
        }
    }

    public class TestWithLoadedPluginBase : Base
    {
        protected async Task<AssemblyLoadContextWithLoadedPluginTestContext> SetupLoadedPluginTextContext(Action<PluginLoadContext> configure = null)
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var assemblyLoadStrategy = testContext.GetMock<IAssemblyLoadStrategy>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var pluginDependencyProvider = testContext.GetMock<IPluginDependencyContextProvider>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            var pluginLoadContext = GetPluginLoadContext(pluginAssemblyPath, configure);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);
            pluginDependencyProvider.Setup(p => p.FromPluginLoadContext(pluginLoadContext)).ReturnsAsync(pluginDependencyContext.Object);

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(pluginLoadContext);

            return new AssemblyLoadContextWithLoadedPluginTestContext
            {
                TestContext = testContext,
                InitialPluginLoadDirectory = initialPluginLoadDirectory,
                PluginAssemblyPath = pluginAssemblyPath,
                PluginLoadContext = pluginLoadContext,
                NewtonsoftAssembly = newtonsoftAssembly,
                NewtonsoftAssemblyName = newtonsoftAssemblyName,
                NewtonsoftAssemblyPath = newtonsoftAssemblyPath,
            };
        }
    }
    public class Base : TestBase
    {
        protected T InvokeProtectedMethodOnLoadContextAndGetResult<T>(IAssemblyLoadContext loadContext, string methodName, params object[] args)
        {
            return (T)loadContext
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(loadContext, args);
        }

        protected IPluginLoadContext GetPluginLoadContext(string pluginAssemblyPath, Action<PluginLoadContext> configure = null)
        {
            var pluginLoadContext = new PluginLoadContext(pluginAssemblyPath, GetContractType(), "netcoreapp3.1");
            configure?.Invoke(pluginLoadContext);
            return pluginLoadContext;
        }

        protected Type GetContractType()
        {
            return TestableTypeBuilder.New()
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
            var pluginDependencyContextProvider = this.mockRepository.Create<IPluginDependencyContextProvider>();

            var loadContext = new DefaultAssemblyLoadContext(
               () => nativeUnloader.Object,
               () => pluginDependencyResolver.Object,
               () => assemblyLoadStrategy.Object,
               (s) => resolver.Object,
               () => fileSystemUtility.Object,
               () => runtimeDefaultAssemblyLoadContext.Object,
               () => pluginDependencyContextProvider.Object);

            return new TestContext<IAssemblyLoadContext>(loadContext,
                nativeUnloader,
                pluginDependencyResolver,
                assemblyLoadStrategy,
                pluginDependencyContext,
                resolver,
                fileSystemUtility,
                runtimeDefaultAssemblyLoadContext,
                pluginDependencyContextProvider
            );
        }
    }
}