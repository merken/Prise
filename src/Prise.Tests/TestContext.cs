using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Prise.Tests
{
    public class TestContext<T>
        where T : class
    {
        private readonly T sut;
        private readonly List<Mock> mocks;

        public TestContext(T sut, params Mock[] mocks)
        {
            this.mocks = new List<Mock>();

            foreach (var mock in mocks)
            {
                this.mocks.Add(mock);
            }

            this.sut = sut;

        }

        public T Sut() => this.sut;

        public Mock<M> GetMock<M>()
            where M : class
        {
            var mock = this.mocks.FirstOrDefault(m => m.Object.GetType().GetTypeInfo().ImplementedInterfaces.ElementAt(0) == typeof(M));
            if (mock == null)
                throw new NotSupportedException($"Mock {typeof(M).Name} not found");

            return mock.As<M>();
        }
    }
}