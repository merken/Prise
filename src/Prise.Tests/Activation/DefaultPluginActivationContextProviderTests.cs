
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.Activation;
using Prise.AssemblyLoading;
using Prise.AssemblyScanning;
using Prise.Core;
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
    }
}