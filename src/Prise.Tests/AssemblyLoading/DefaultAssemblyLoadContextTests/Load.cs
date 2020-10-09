using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyLoading;
using Prise.Core;
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
    public class Load : Base
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
            // var result = loadContext.LoadFromAssemblyPath(assembly.Location);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Load_Works()
        {
            var testContext = SetupAssemblyLoadContext();
            var loadContext = testContext.Sut();
            var fileSystemUtility = testContext.GetMock<IFileSystemUtilities>();
            var assemblyLoadStrategy = testContext.GetMock<IAssemblyLoadStrategy>();
            var pluginDependencyContext = testContext.GetMock<IPluginDependencyContext>();

            var pluginAssemblyPath = Path.Combine(GetPathToAssemblies(), "Prise.Tests.dll");
            var initialPluginLoadDirectory = Path.GetDirectoryName(pluginAssemblyPath);

            var newtonsoftAssemblyPath = Path.Combine(GetPathToAssemblies(), "Newtonsoft.Json.dll");
            var newtonsoftAssembly = Assembly.LoadFile(newtonsoftAssemblyPath);
            var newtonsoftAssemblyName = AssemblyLoadContext.GetAssemblyName(newtonsoftAssemblyPath);
            var assemblyStream = File.OpenRead(pluginAssemblyPath);

            fileSystemUtility.Setup(f => f.EnsureFileExists(pluginAssemblyPath)).Returns(pluginAssemblyPath);
            fileSystemUtility.Setup(f => f.ReadFileFromDisk(pluginAssemblyPath)).ReturnsAsync(assemblyStream);

            assemblyLoadStrategy.Setup(a => a.LoadAssembly(initialPluginLoadDirectory, newtonsoftAssemblyName, pluginDependencyContext.Object,
                It.IsAny<Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>>>(),
                It.IsAny<Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>>>(),
                It.IsAny<Func<string, AssemblyName, ValueOrProceed<AssemblyFromStrategy>>>())).Returns(new AssemblyFromStrategy
                {
                    Assembly = newtonsoftAssembly,
                    CanBeReleased = true
                });

            // This must be invoked before anything else can be tested
            await loadContext.LoadPluginAssembly(GetPluginLoadContext(pluginAssemblyPath));
            var result = loadContext.GetType().GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(loadContext, new[] { newtonsoftAssemblyName }) as Assembly;

            Assert.IsNotNull(result);
        }
    }
}