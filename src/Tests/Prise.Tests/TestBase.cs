using System;
using System.Collections.Generic;
using AutoFixture;
using Moq;

namespace Prise.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected readonly Fixture fixture = new Fixture();
        protected readonly MockRepository moq = new MockRepository(MockBehavior.Strict);
        protected readonly Dictionary<string, Mock> moqs = new Dictionary<string, Mock>();

        protected T CreateFixture<T>() where T : class => this.fixture.Create<T>();
        protected T LooseMock<T>() where T : class => AddToMoqs(new Mock<T>(MockBehavior.Loose)).Object;
        protected T Mock<T>() where T : class => AddToMoqs(this.moq.Create<T>()).Object;
        protected Mock<T> Arrange<T>() where T : class => GetOrAdd<T>();
        protected void Verify() => this.moq.VerifyAll();

        private Mock<T> GetOrAdd<T>() where T : class
        {
            var key = typeof(T).Name;
            if (this.moqs.ContainsKey(key))
                return (Mock<T>)this.moqs[key];

            return this.moq.Create<T>();
        }

        private Mock<T> AddToMoqs<T>(Mock<T> mock) where T : class
        {
            this.moqs[typeof(T).Name] = mock;
            return mock;
        }

        public void Dispose()
        {
            this.moq.VerifyAll();
        }
    }
}
