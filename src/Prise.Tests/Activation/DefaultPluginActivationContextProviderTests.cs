
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;

using Prise.Plugin;
using Prise.Tests.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests.Activation
{
    [TestClass]
    public class DefaultPluginActivationContextProviderTests : TestBase
    {
        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultPluginActivationContextProvider());
        }

        //TODO More Tests

        private Type GetContract()
        {
            return TestableTypeBuilder.New()
                   .WithName("IMyTestType")
                   .WithNamespace("Test.Type")
                   .Build();
        }

        private CustomAttributeData GetPluginAttributeForContract(Type contract)
        {
            return TestableAttributeBuilder.New()
                                    .WithAttributeType(typeof(Prise.Plugin.PluginAttribute))
                                    .WithNamedAgrument("PluginType", contract)
                                    .Build();
        }

        [TestMethod]
        public void ProvideActivationContext_Works()
        {
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var contract = GetContract();
            var pluginAttribute = GetPluginAttributeForContract(contract);

            var typeName = "MyTestType";
            var testableType = TestableTypeBuilder.New()
                .WithCustomAttributes(pluginAttribute)
                .WithName(typeName)
                .WithNamespace("Test.Type")
                .Build();

            // NO factory method
            // NO activated method
            // NO bootstrapper
            // NO services

            assemblyShim.Setup(a => a.Types).Returns(new[] { testableType });

            var sut = new DefaultPluginActivationContextProvider();
            var result = sut.ProvideActivationContext(testableType, assemblyShim.Object);

            Assert.IsNotNull(result);
            Assert.IsNull(result.PluginActivatedMethod);
            Assert.IsNull(result.PluginFactoryMethod);
            Assert.AreEqual(0, result.PluginServices.Count());
            Assert.AreEqual(typeName, result.PluginType.Name);
        }

        [TestMethod]
        public void ProvideActivationContext_WithFactoryMethod_Works()
        {
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var contract = GetContract();
            var pluginAttribute = GetPluginAttributeForContract(contract);

            // Factory method
            var factoryMethodName = "This_Is_The_Factory_Method";
            var factoryMethod = TestableMethodInfoBuilder.New()
                .WithName(factoryMethodName)
                .WithAttribute(TestableAttributeBuilder.New()
                    .WithAttributeType(typeof(Prise.Plugin.PluginFactoryAttribute)).Build())
                .Build();

            var typeName = "MyTestType";
            var testableType = TestableTypeBuilder.New()
                .WithCustomAttributes(pluginAttribute)
                .WithName(typeName)
                .WithNamespace("Test.Type")
                .WithMethods(factoryMethod)
                .Build();

            // NO activated method
            // NO bootstrapper
            // NO services

            assemblyShim.Setup(a => a.Types).Returns(new[] { testableType });

            var sut = new DefaultPluginActivationContextProvider();
            var result = sut.ProvideActivationContext(testableType, assemblyShim.Object);

            Assert.IsNotNull(result);
            Assert.IsNull(result.PluginActivatedMethod);
            Assert.IsNotNull(result.PluginFactoryMethod);
            Assert.AreEqual(factoryMethodName, result.PluginFactoryMethod.Name);
            Assert.AreEqual(0, result.PluginServices.Count());
            Assert.AreEqual(typeName, result.PluginType.Name);
        }

        [TestMethod]
        public void ProvideActivationContext_WithActivatedMethod_Works()
        {
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var contract = GetContract();
            var pluginAttribute = GetPluginAttributeForContract(contract);

            // Factory method
            var factoryMethodName = "This_Is_The_Factory_Method";
            var factoryMethod = TestableMethodInfoBuilder.New()
                .WithName(factoryMethodName)
                .WithAttribute(TestableAttributeBuilder.New()
                    .WithAttributeType(typeof(Prise.Plugin.PluginFactoryAttribute)).Build())
                .Build();

            // Activated method
            var activatedMethodName = "This_Is_The_Activation_Method";
            var activatedMethod = TestableMethodInfoBuilder.New()
                .WithName(activatedMethodName)
                .WithAttribute(TestableAttributeBuilder.New()
                    .WithAttributeType(typeof(Prise.Plugin.PluginActivatedAttribute)).Build())
                .Build();

            var typeName = "MyTestType";
            var testableType = TestableTypeBuilder.New()
                .WithCustomAttributes(pluginAttribute)
                .WithName(typeName)
                .WithNamespace("Test.Type")
                .WithMethods(new[] { factoryMethod, activatedMethod })
                .Build();

            // NO bootstrapper
            // NO services

            assemblyShim.Setup(a => a.Types).Returns(new[] { testableType });

            var sut = new DefaultPluginActivationContextProvider();
            var result = sut.ProvideActivationContext(testableType, assemblyShim.Object);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PluginActivatedMethod);
            Assert.AreEqual(activatedMethodName, result.PluginActivatedMethod.Name);
            Assert.IsNotNull(result.PluginFactoryMethod);
            Assert.AreEqual(factoryMethodName, result.PluginFactoryMethod.Name);
            Assert.AreEqual(0, result.PluginServices.Count());
            Assert.AreEqual(typeName, result.PluginType.Name);
        }

        [TestMethod]
        public void ProvideActivationContext_WithPluginServices_Works()
        {
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var contract = GetContract();
            var pluginAttribute = GetPluginAttributeForContract(contract);

            // Factory method
            var factoryMethodName = "This_Is_The_Factory_Method";
            var factoryMethod = TestableMethodInfoBuilder.New()
                .WithName(factoryMethodName)
                .WithAttribute(TestableAttributeBuilder.New()
                    .WithAttributeType(typeof(Prise.Plugin.PluginFactoryAttribute)).Build())
                .Build();

            // Activated method
            var activatedMethodName = "This_Is_The_Activation_Method";
            var activatedMethod = TestableMethodInfoBuilder.New()
                .WithName(activatedMethodName)
                .WithAttribute(TestableAttributeBuilder.New()
                    .WithAttributeType(typeof(Prise.Plugin.PluginActivatedAttribute)).Build())
                .Build();


            var serviceTypeName1 = "IMyService1";
            var serviceType1 = TestableTypeBuilder.New()
                .WithName(serviceTypeName1)
                .WithNamespace("Test.Type")
                .Build();

            var pluginService1 = "pluginService";
            var serviceField1 = TestableFieldBuilder.New()
                .WithName(pluginService1)
                .WithAttribute(TestableAttributeBuilder.New()
                    .WithAttributeType(typeof(Prise.Plugin.PluginServiceAttribute))
                    .WithNamedAgrument("ServiceType", serviceType1)
                    .WithNamedAgrument("ProvidedBy", ProvidedBy.Plugin)
                    .Build())
                .Build();

            var proxyTypeName = "IProxyType";
            var proxyType = TestableTypeBuilder.New()
                .WithName(proxyTypeName)
                .WithNamespace("Test.Type")
                .Build();

            var pluginService2 = "hostService";
            var serviceField2 = TestableFieldBuilder.New()
                .WithName(pluginService2)
                .WithAttribute(TestableAttributeBuilder.New()
                    .WithAttributeType(typeof(Prise.Plugin.PluginServiceAttribute))
                    .WithNamedAgrument("ServiceType", serviceType1)
                    .WithNamedAgrument("ProvidedBy", ProvidedBy.Host)
                    .WithNamedAgrument("ProxyType", proxyType)
                    .Build())
                .Build();

            var typeName = "MyTestType";
            var testableType = TestableTypeBuilder.New()
                .WithCustomAttributes(pluginAttribute)
                .WithName(typeName)
                .WithNamespace("Test.Type")
                .WithMethods(new[] { factoryMethod, activatedMethod })
                .WithFields(new[] { serviceField1, serviceField2 })
                .Build();

            // NO bootstrapper

            assemblyShim.Setup(a => a.Types).Returns(new[] { testableType });

            var sut = new DefaultPluginActivationContextProvider();
            var result = sut.ProvideActivationContext(testableType, assemblyShim.Object);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PluginActivatedMethod);
            Assert.AreEqual(activatedMethodName, result.PluginActivatedMethod.Name);
            Assert.IsNotNull(result.PluginFactoryMethod);
            Assert.AreEqual(factoryMethodName, result.PluginFactoryMethod.Name);
            Assert.AreEqual(2, result.PluginServices.Count());
            Assert.AreEqual(pluginService1, result.PluginServices.ElementAt(0).FieldName);
            Assert.AreEqual(pluginService2, result.PluginServices.ElementAt(1).FieldName);
            Assert.AreEqual(ProvidedBy.Plugin, result.PluginServices.ElementAt(0).ProvidedBy);
            Assert.AreEqual(ProvidedBy.Host, result.PluginServices.ElementAt(1).ProvidedBy);
            Assert.IsNull(result.PluginServices.ElementAt(0).ProxyType);
            Assert.IsNotNull(result.PluginServices.ElementAt(1).ProxyType);
            Assert.AreEqual(proxyTypeName, result.PluginServices.ElementAt(1).ProxyType.Name);
            Assert.AreEqual(serviceTypeName1, result.PluginServices.ElementAt(0).ServiceType.Name);
            Assert.AreEqual(serviceTypeName1, result.PluginServices.ElementAt(1).ServiceType.Name);
            Assert.AreEqual(typeName, result.PluginType.Name);
        }

        [TestMethod]
        public void ProvideActivationContext_WithBootstrapper_Works()
        {
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var contract = GetContract();
            var pluginAttribute = GetPluginAttributeForContract(contract);

            var serviceTypeName = "IMyService1";
            var serviceType = TestableTypeBuilder.New()
                .WithName(serviceTypeName)
                .WithNamespace("Test.Type")
                .Build();

            var proxyTypeName = "BootstrapperService";
            var proxyType = TestableTypeBuilder.New()
                .WithName(proxyTypeName)
                .WithNamespace("Test.Type")
                .Build();

            var bootstrapperServiceFieldName = "bootstrapperService";
            var bootstrapperServiceField = TestableFieldBuilder.New()
                .WithName(bootstrapperServiceFieldName)
                .WithAttribute(TestableAttributeBuilder.New()
                    .WithAttributeType(typeof(Prise.Plugin.BootstrapperServiceAttribute))
                    .WithNamedAgrument("ServiceType", serviceType)
                    .WithNamedAgrument("ProxyType", proxyType)
                    .Build())
                .Build();

            var typeName = "MyTestType";
            var testableType = TestableTypeBuilder.New()
                .WithCustomAttributes(pluginAttribute)
                .WithName(typeName)
                .WithNamespace("Test.Type")
                .Build();

            var bootstrapperAttribute = TestableAttributeBuilder.New()
                                    .WithAttributeType(typeof(Prise.Plugin.PluginBootstrapperAttribute))
                                    .WithNamedAgrument("PluginType", testableType)
                                    .Build();

            var bootstrapperName = "MyTestTypeBootstrapper";
            var bootstrapperType = TestableTypeBuilder.New()
                .WithCustomAttributes(bootstrapperAttribute)
                .WithName(bootstrapperName)
                .WithNamespace("Test.Type")
                .WithFields(new[] { bootstrapperServiceField })
                .Build();

            assemblyShim.Setup(a => a.Types).Returns(new[] { testableType, bootstrapperType });

            var sut = new DefaultPluginActivationContextProvider();
            var result = sut.ProvideActivationContext(testableType, assemblyShim.Object);

            Assert.IsNotNull(result);
            Assert.IsNull(result.PluginActivatedMethod);
            Assert.IsNull(result.PluginFactoryMethod);
            Assert.AreEqual(0, result.PluginServices.Count());
            Assert.AreEqual(bootstrapperServiceFieldName, result.BootstrapperServices.ElementAt(0).FieldName);
            Assert.AreEqual(proxyTypeName, result.BootstrapperServices.ElementAt(0).ProxyType.Name);
            Assert.AreEqual(serviceTypeName, result.BootstrapperServices.ElementAt(0).ServiceType.Name);
            Assert.AreEqual(typeName, result.PluginType.Name);
            Assert.AreEqual(bootstrapperName, result.PluginBootstrapperType.Name);
        }
    }
}