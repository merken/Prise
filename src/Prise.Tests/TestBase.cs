using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Prise.Tests
{
    public abstract class TestBase
    {
        protected MockRepository mockRepository;

        public TestBase()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        protected string GetPathToAssemblies()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assemblies");
        }

        [TestCleanup()]
        public void Cleanup()
        {
            this.mockRepository.VerifyAll();
        }
    }
}