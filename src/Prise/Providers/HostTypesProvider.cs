using Prise.Infrastructure;
using System;
using System.Collections.Generic;

namespace Prise
{
    // TODO this must be plugin context specific
    public class HostTypesProvider<T> : IHostTypesProvider<T>
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

        public HostTypesProvider<T> AddHostType(Type type)
        {
            this.hostTypes.Add(type);
            return this;
        }

        public HostTypesProvider<T> AddHostType<THostType>() => AddHostType(typeof(THostType));

        public IEnumerable<Type> ProvideHostTypes() => this.hostTypes;
    }
}
