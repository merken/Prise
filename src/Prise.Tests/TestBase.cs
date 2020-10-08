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

        [TestCleanup()]
        public void Cleanup()
        {
            this.mockRepository.VerifyAll();
        }
    }
}