using Prise.Infrastructure;
using System;
using System.Collections.Generic;

namespace Prise
{
    public class HostTypesProvider : IHostTypesProvider
    {
        private IList<Type> hostTypes;
        protected bool disposed = false;
        public HostTypesProvider()
        {
            this.hostTypes = new List<Type>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // nothing to do
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public HostTypesProvider AddHostType(Type type)
        {
            this.hostTypes.Add(type);
            return this;
        }

        public HostTypesProvider AddHostType<T>() => AddHostType(typeof(T));

        public IEnumerable<Type> ProvideHostTypes() => this.hostTypes;
    }
}
