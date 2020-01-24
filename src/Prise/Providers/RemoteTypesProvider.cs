
using Prise.Infrastructure;
using System;
using System.Collections.Generic;

namespace Prise
{
    // TODO this must be plugin context specific
    public class RemoteTypesProvider<T> : IRemoteTypesProvider<T>
    {
        private IList<Type> remoteTypes;
        protected bool disposed = false;
        public RemoteTypesProvider()
        {
            this.remoteTypes = new List<Type>();
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

        public RemoteTypesProvider<T> AddRemoteType(Type type)
        {
            this.remoteTypes.Add(type);
            return this;
        }

        public RemoteTypesProvider<T> AddRemoteType<TType>() => AddRemoteType(typeof(TType));

        public IEnumerable<Type> ProvideRemoteTypes() => this.remoteTypes;
    }
}