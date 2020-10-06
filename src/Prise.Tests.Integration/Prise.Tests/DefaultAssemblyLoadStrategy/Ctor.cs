using System;
using Xunit;
using Prise;
using Prise.Infrastructure;

namespace Prise.Tests.DefaultAssemblyLoadStrategy
{
    public class Ctor : TestBase
    {
        [Fact]
        public void IPluginLogger_Required()
        {
            Assert.Throws<ArgumentNullException>(() => new Prise.DefaultAssemblyLoadStrategy(null, null, null));
        }

        [Fact]
        public void IPluginLoadContext_Required()
        {
            Assert.Throws<ArgumentNullException>(() => new Prise.DefaultAssemblyLoadStrategy(this.Mock<IPluginLogger>(), null, null));
        }

        [Fact]
        public void IPluginDependencyContext_Required()
        {
            Assert.Throws<ArgumentNullException>(() => new Prise.DefaultAssemblyLoadStrategy(this.Mock<IPluginLogger>(), this.Mock<IPluginLoadContext>(), null));
        }

        [Fact]
        public void Succeeds()
        {
            Assert.NotNull(new Prise.DefaultAssemblyLoadStrategy(
                this.Mock<IPluginLogger>(),
                this.Mock<IPluginLoadContext>(),
                this.Mock<IPluginDependencyContext>()));
        }
    }
}
