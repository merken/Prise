using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prise.AssemblyScanning;
using Prise.Caching;
using Prise.Tests.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Tests.Caching
{
    [TestClass]
    public class DefaultScopedPluginCacheTests : TestBase
    {
        [TestMethod]
        public void Ctor_Works()
        {
            Assert.IsNotNull(new DefaultScopedPluginCache());
        }

        [TestMethod]
        public void Add_Works()
        {
            var assemblyShim = this.mockRepository.Create<IAssemblyShim>();
            var types = new List<Type>
            {
                typeof(DefaultScopedPluginCacheTests)
            };
            var cache = new DefaultScopedPluginCache();
            cache.Add(assemblyShim.Object, types);

            var itemInCache = cache.GetAll().SingleOrDefault();
            Assert.IsNotNull(itemInCache);
            Assert.AreEqual(assemblyShim.Object, itemInCache.AssemblyShim);
            Assert.AreEqual(types.First(), itemInCache.HostTypes.First());
        }
    }
}