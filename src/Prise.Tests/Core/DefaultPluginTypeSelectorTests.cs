using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyScanning;
using Prise.Caching;
using Prise.Core;
using Prise.Tests.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests.Core
{
    [TestClass]
    public class DefaultPluginTypeSelectorTests : TestBase
    {
        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultPluginTypeSelector());
        }

        [TestMethod]
        public void Selecting_Plugin_Types_Works()
        {
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var contract = TestableTypeBuilder.NewTestableType()
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

            var testableType = TestableTypeBuilder.NewTestableType()
                .WithCustomAttributes(pluginAttribute)
                .WithName("MyTestType")
                .WithNamespace("Test.Type")
                .Build();

            var testableType2 = TestableTypeBuilder.NewTestableType()
                .WithCustomAttributes(pluginAttribute)
                .WithName("MyTestType2")
                .WithNamespace("Test.Type")
                .Build();

            assemblyShim.Setup(a => a.Types).Returns(new[] { testableType2, testableType, typeof(ITestPlugin) });

            var selector = new DefaultPluginTypeSelector();
            var pluginsTypes = selector.SelectPluginTypes(contract, assemblyShim.Object);

            Assert.AreEqual(2, pluginsTypes.Count());
            Assert.AreEqual("MyTestType", pluginsTypes.First().Name);
            Assert.AreEqual("MyTestType2", pluginsTypes.ElementAt(1).Name);
        }
    }
}