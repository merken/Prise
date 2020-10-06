using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyScanning;
using Prise.Tests.Plugins;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests.AssemblyScanning
{
    [TestClass]
    public class DefaultAssemblyScannerTests
    {
        private MockRepository mockRepository;

        public DefaultAssemblyScannerTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TestMethod]
        public void Ctor_No_Factory_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultAssemblyScanner(null));
        }

        [TestMethod]
        public async Task Scan_No_Options_Throws_ArgumentNullException()
        {
            var scanner = new DefaultAssemblyScanner((s) => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => scanner.Scan(null));
        }

        [TestMethod]
        public async Task Scan_No_StartingPath_Throws_ArgumentException()
        {
            var scanner = new DefaultAssemblyScanner((s) => null);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                PluginType = typeof(ITestPlugin)
            }));
        }

        [TestMethod]
        public async Task Scan_No_PluginType_Throws_ArgumentException()
        {
            var scanner = new DefaultAssemblyScanner((s) => null);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = "/home/maarten"
            }));
        }

        [TestMethod]
        public async Task Scan_Unrooted_Path_Throws_AssemblyScanningException()
        {
            var scanner = new DefaultAssemblyScanner((s) => null);
            await Assert.ThrowsExceptionAsync<AssemblyScanningException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = "../home/maarten",
                PluginType = typeof(ITestPlugin)
            }));
        }

        [TestMethod]
        public async Task Scan_Succeeds()
        {
            var metadataLoadContext = this.mockRepository.Create<IMetadataLoadContext>();
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var scanner = new DefaultAssemblyScanner((s) => metadataLoadContext.Object);

            var pluginAttributeTypedValue = new CustomAttributeTypedArgument(typeof(Prise.Plugin.PluginAttribute));
            var pluginAttribute = new TestableAttribute
            {
                _AttributeType = typeof(Prise.Plugin.PluginAttribute),
                _NamedArguments = new[]{new CustomAttributeNamedArgument(new TestableMemberInfo{
                    _Name = "PluginType"
                },pluginAttributeTypedValue)}
            };
            
            var testableType = TestableTypeBuilder.NewTestableType()
                .WithCustomAttributes(pluginAttribute)
                .WithName("MyTestType")
                .WithNamespace("Test.Type")
                .Build();

            assemblyShim.Setup(a => a.Types).Returns(new[] { testableType });

            metadataLoadContext.Setup(c => c.LoadFromAssemblyName(It.IsAny<string>())).Returns(assemblyShim.Object);

            var types = await scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = "/home/maarten",
                PluginType = testableType
            });

            var result = types.FirstOrDefault();
            Assert.IsNotNull(result);
            Assert.AreEqual("MyTestType", result.PluginType.Name);
            Assert.AreEqual("Test.Type", result.PluginType.Namespace);
        }

        private static string GetPathToBinDebug()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            return Path.GetDirectoryName(pathToThisProgram);
        }
    }
}