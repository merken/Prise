using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyLoading;

using Prise.Tests.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Prise.Tests.AssemblyLoading.DefaultAssemblyLoadContextTests
{
    [TestClass]
    public class DefaultAssemblyLoadContextTests_Load : TestWithLoadedPluginBase
    {
        [TestMethod]
        public async Task Disposing_Prevents_Load()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();

            var assembly = this.GetType().Assembly;
            var assemblyName = AssemblyLoadContext.GetAssemblyName(assembly.Location);

            loadContext.Dispose();
            var result = loadContext.GetType().GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { assemblyName }) as Assembly;
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Load_Works()
        {
            var testContext = await SetupLoadedPluginTextContext();
            var loadContext = testContext.Sut();
            var assemblyLoadStrategy = testContext.GetMock<IAssemblyLoadStrategy>();
            var initialPluginLoadDirectory = testContext.InitialPluginLoadDirectory;
            var pluginLoadContext = testContext.PluginLoadContext;
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();
            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);

            assemblyLoadStrategy.Setup(a => a.LoadAssembly(initialPluginLoadDirectory, newtonsoftAssemblyName, pluginDependencyContext.Object,
                It.IsAny<Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>>>(),
                It.IsAny<Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>>>(),
                It.IsAny<Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>>>())).Returns(new AssemblyFromStrategy
                {
                    Assembly = newtonsoftAssembly,
                    CanBeReleased = true
                });

            var result = InvokeProtectedMethodOnLoadContextAndGetResult<Assembly>(
                loadContext,
                "Load",
                new object[] { newtonsoftAssemblyName });

            Assert.IsNotNull(result);
        }
    }
}