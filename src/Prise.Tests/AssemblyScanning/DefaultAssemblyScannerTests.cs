using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyScanning;
using Prise.Core;
using Prise.Tests.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests.AssemblyScanning
{
    [TestClass]
    public class DefaultAssemblyScannerTests : TestBase
    {
        [TestMethod]
        public void Ctor_No_MetadataLoadContextFactory_Throws_ArgumentNullException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyScanner(null, null));
            exception.Message.Contains("metadataLoadContextFactory");
        }

        [TestMethod]
        public void Ctor_No_DirectoryTraverserFactory_Throws_ArgumentNullException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyScanner((s) => null, null));
            exception.Message.Contains("directoryTraverser");
        }

        [TestMethod]
        public async Task Scan_No_Options_Throws_ArgumentNullException()
        {
            var scanner = new DefaultAssemblyScanner((s) => null, () => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => scanner.Scan(null));
        }

        [TestMethod]
        public async Task Scan_No_StartingPath_Throws_ArgumentException()
        {
            var scanner = new DefaultAssemblyScanner((s) => null, () => null);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                PluginType = typeof(ITestPlugin)
            }));
        }

        [TestMethod]
        public async Task Scan_No_PluginType_Throws_ArgumentException()
        {
            var scanner = new DefaultAssemblyScanner((s) => null, () => null);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = "/home/maarten"
            }));
        }

        [TestMethod]
        public async Task Scan_Unrooted_Path_Throws_AssemblyScanningException()
        {
            var scanner = new DefaultAssemblyScanner((s) => null, () => null);
            await Assert.ThrowsExceptionAsync<AssemblyScanningException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = "../home/maarten",
                PluginType = typeof(ITestPlugin)
            }));
        }

        [TestMethod]
        public async Task Scan_Succeeds()
        {
            var startingPath = "/home/maarten";
            var metadataLoadContext = this.mockRepository.Create<IMetadataLoadContext>();
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();

            directoryTraverser.Setup(d => d.TraverseDirectories(startingPath)).Returns(new[] { "pathy/mcpathface" });
            directoryTraverser.Setup(d => d.TraverseFiles(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(new[] { "filey.mcfile.face" });

            var contract = TestableTypeBuilder.New()
                .WithName("IMyTestType")
                .WithNamespace("Test.Type")
                .Build();

            var pluginAttributeTypedValue = new CustomAttributeTypedArgument(contract);
            var pluginAttribute = new TestableAttribute
            {
                _AttributeType = typeof(Prise.Plugin.PluginAttribute),
                _NamedArguments = new[]{new CustomAttributeNamedArgument(new TestableMemberInfo{
                    _Name = "PluginType"
                },pluginAttributeTypedValue)}
            };

            var testableType = TestableTypeBuilder.New()
                .WithCustomAttributes(pluginAttribute)
                .WithName("MyTestType")
                .WithNamespace("Test.Type")
                .Build();

            assemblyShim.Setup(a => a.Types).Returns(new[] { testableType });

            metadataLoadContext.Setup(c => c.LoadFromAssemblyName(It.IsAny<string>())).Returns(assemblyShim.Object);

            var scanner = new DefaultAssemblyScanner(
                (s) => metadataLoadContext.Object,
                () => directoryTraverser.Object
            );
            var types = await scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = startingPath,
                PluginType = contract
            });

            var result = types.FirstOrDefault();
            Assert.IsNotNull(result);
            Assert.AreEqual("MyTestType", result.PluginType.Name);
            Assert.AreEqual("Test.Type", result.PluginType.Namespace);
        }
    }
}