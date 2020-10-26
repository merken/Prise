using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyScanning;

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
    public class DefaultNugetPackageAssemblyScannerTests : TestBase
    {
        [TestMethod]
        public void Ctor_No_NugetUtilitiesFactory_Throws_ArgumentNullException()
        {
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new DefaultNugetPackageAssemblyScanner((s) => null, () => null, null));
            exception.Message.Contains("nugetPackageUtilities");
        }

        [TestMethod]
        public async Task Scan_No_Options_Throws_ArgumentNullException()
        {
            var scanner = new DefaultNugetPackageAssemblyScanner((s) => null, () => null, () => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => scanner.Scan(null));
        }

        [TestMethod]
        public async Task Scan_No_StartingPath_Throws_ArgumentException()
        {
            var scanner = new DefaultNugetPackageAssemblyScanner((s) => null, () => null, () => null);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                PluginType = typeof(ITestPlugin)
            }));
        }

        [TestMethod]
        public async Task Scan_No_PluginType_Throws_ArgumentException()
        {
            var scanner = new DefaultNugetPackageAssemblyScanner((s) => null, () => null, () => null);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = "/home/maarten"
            }));
        }

        [TestMethod]
        public async Task Scan_Unrooted_Path_Throws_AssemblyScanningException()
        {
            var scanner = new DefaultNugetPackageAssemblyScanner((s) => null, () => null, () => null);
            await Assert.ThrowsExceptionAsync<AssemblyScanningException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = "../home/maarten",
                PluginType = typeof(ITestPlugin)
            }));
        }


        [TestMethod]
        public async Task Scan_No_Nugets_Throws_AssemblyScanningException()
        {
            var startingPath = "/home/maarten";
            var nugetUtilities = this.mockRepository.Create<INugetPackageUtilities>();
            nugetUtilities.Setup(n => n.FindAllNugetPackagesFiles(startingPath)).Returns(Enumerable.Empty<string>());
            var scanner = new DefaultNugetPackageAssemblyScanner((s) => null, () => null, () => nugetUtilities.Object);

            await Assert.ThrowsExceptionAsync<AssemblyScanningException>(() => scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = startingPath,
                PluginType = typeof(ITestPlugin)
            }));
        }

        [TestMethod]
        public async Task Compressed_NugetPackage_Must_Be_UnCompressed()
        {
            var package = "Prise.Plugin.Package.1.0.0.nupkg";
            var startingPath = "/home/maarten";
            var nugetUtilities = this.mockRepository.Create<INugetPackageUtilities>();
            var metadataLoadContext = this.mockRepository.Create<IMetadataLoadContext>();
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();
            var actualNugetUtilities = new DefaultNugetPackageUtilities();

            nugetUtilities.Setup(n => n.FindAllNugetPackagesFiles(startingPath)).Returns(new[] { package });
            // Call actual implementation here
            nugetUtilities.Setup(n => n.GetVersionFromPackageFile(It.IsAny<string>())).Returns<string>((s) => actualNugetUtilities.GetVersionFromPackageFile(s));
            // Call actual implementation here
            nugetUtilities.Setup(n => n.GetPackageName(It.IsAny<string>())).Returns<string>((s) => actualNugetUtilities.GetPackageName(s));
            nugetUtilities.Setup(n => n.HasAlreadyBeenExtracted(It.IsAny<string>())).Returns(false);
            nugetUtilities.Setup(n => n.UnCompressNugetPackage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            directoryTraverser.Setup(d => d.TraverseDirectories($"{startingPath}/_extracted")).Returns(new[] { "pathy/mcpathface" });
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

            var scanner = new DefaultNugetPackageAssemblyScanner(
                (s) => metadataLoadContext.Object,
                () => directoryTraverser.Object,
                () => nugetUtilities.Object
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

        [TestMethod]
        public async Task UnCompressed_NugetPackage_Does_Nothing()
        {
            var package = "Prise.Plugin.Package.1.0.0.nupkg";
            var startingPath = "/home/maarten";
            var nugetUtilities = this.mockRepository.Create<INugetPackageUtilities>();
            var metadataLoadContext = this.mockRepository.Create<IMetadataLoadContext>();
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();
            var actualNugetUtilities = new DefaultNugetPackageUtilities();

            nugetUtilities.Setup(n => n.FindAllNugetPackagesFiles(startingPath)).Returns(new[] { package });
            // Call actual implementation here
            nugetUtilities.Setup(n => n.GetVersionFromPackageFile(It.IsAny<string>())).Returns<string>((s) => actualNugetUtilities.GetVersionFromPackageFile(s));
            // Call actual implementation here
            nugetUtilities.Setup(n => n.GetPackageName(It.IsAny<string>())).Returns<string>((s) => actualNugetUtilities.GetPackageName(s));
            nugetUtilities.Setup(n => n.HasAlreadyBeenExtracted(It.IsAny<string>())).Returns(true);

            directoryTraverser.Setup(d => d.TraverseDirectories($"{startingPath}/_extracted")).Returns(new[] { "pathy/mcpathface" });
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

            var scanner = new DefaultNugetPackageAssemblyScanner(
                (s) => metadataLoadContext.Object,
                () => directoryTraverser.Object,
                () => nugetUtilities.Object
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

        [TestMethod]
        public async Task Multiple_Versions_With_Newer_Available_Deletes_Current_And_Extracts_New()
        {
            var currentVersion = new Version("1.0.0");
            var package1 = "Prise.Plugin.Package.1.0.0.nupkg";
            var package2 = "Prise.Plugin.Package.1.1.0.nupkg";
            var startingPath = "/home/maarten";
            var nugetUtilities = this.mockRepository.Create<INugetPackageUtilities>();
            var metadataLoadContext = this.mockRepository.Create<IMetadataLoadContext>();
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();
            var actualNugetUtilities = new DefaultNugetPackageUtilities();

            nugetUtilities.Setup(n => n.FindAllNugetPackagesFiles(startingPath)).Returns(new[] { package1, package2 });
            // Call actual implementation here
            nugetUtilities.Setup(n => n.GetVersionFromPackageFile(It.IsAny<string>())).Returns<string>((s) => actualNugetUtilities.GetVersionFromPackageFile(s));
            // Call actual implementation here
            nugetUtilities.Setup(n => n.GetPackageName(It.IsAny<string>())).Returns<string>((s) => actualNugetUtilities.GetPackageName(s));
            nugetUtilities.Setup(n => n.GetCurrentVersionFromExtractedNuget(It.IsAny<string>())).Returns(currentVersion);
            nugetUtilities.Setup(n => n.HasAlreadyBeenExtracted(It.IsAny<string>())).Returns(true);
            nugetUtilities.Setup(n => n.DeleteNugetDirectory(It.IsAny<string>())).Verifiable();
            nugetUtilities.Setup(n => n.UnCompressNugetPackage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            directoryTraverser.Setup(d => d.TraverseDirectories($"{startingPath}/_extracted")).Returns(new[] { "pathy/mcpathface" });
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

            var scanner = new DefaultNugetPackageAssemblyScanner(
                (s) => metadataLoadContext.Object,
                () => directoryTraverser.Object,
                () => nugetUtilities.Object
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

        [TestMethod]
        public async Task Multiple_Versions_With_Current_Already_Extracted_Does_Nothing()
        {
            var currentVersion = new Version("1.1.0");
            var package1 = "Prise.Plugin.Package.1.0.0.nupkg";
            var package2 = "Prise.Plugin.Package.1.1.0.nupkg";
            var startingPath = "/home/maarten";
            var nugetUtilities = this.mockRepository.Create<INugetPackageUtilities>();
            var metadataLoadContext = this.mockRepository.Create<IMetadataLoadContext>();
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();
            var actualNugetUtilities = new DefaultNugetPackageUtilities();

            nugetUtilities.Setup(n => n.FindAllNugetPackagesFiles(startingPath)).Returns(new[] { package1, package2 });
            // Call actual implementation here
            nugetUtilities.Setup(n => n.GetVersionFromPackageFile(It.IsAny<string>())).Returns<string>((s) => actualNugetUtilities.GetVersionFromPackageFile(s));
            // Call actual implementation here
            nugetUtilities.Setup(n => n.GetPackageName(It.IsAny<string>())).Returns<string>((s) => actualNugetUtilities.GetPackageName(s));
            nugetUtilities.Setup(n => n.GetCurrentVersionFromExtractedNuget(It.IsAny<string>())).Returns(currentVersion);
            nugetUtilities.Setup(n => n.HasAlreadyBeenExtracted(It.IsAny<string>())).Returns(true);

            directoryTraverser.Setup(d => d.TraverseDirectories($"{startingPath}/_extracted")).Returns(new[] { "pathy/mcpathface" });
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

            var scanner = new DefaultNugetPackageAssemblyScanner(
                (s) => metadataLoadContext.Object,
                () => directoryTraverser.Object,
                () => nugetUtilities.Object
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

        [TestMethod]
        public async Task Scan_Succeeds()
        {
            var metadataLoadContext = this.mockRepository.Create<IMetadataLoadContext>();
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var directoryTraverser = this.mockRepository.Create<IDirectoryTraverser>();
            var scanner = new DefaultAssemblyScanner(
                (s) => metadataLoadContext.Object,
                () => directoryTraverser.Object
            );

            directoryTraverser.Setup(d => d.TraverseDirectories(It.IsAny<string>())).Returns(new[] { "pathy/mcpathface" });
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

            var types = await scanner.Scan(new AssemblyScannerOptions
            {
                StartingPath = "/home/maarten",
                PluginType = contract
            });

            var result = types.FirstOrDefault();
            Assert.IsNotNull(result);
            Assert.AreEqual("MyTestType", result.PluginType.Name);
            Assert.AreEqual("Test.Type", result.PluginType.Namespace);
        }
    }
}