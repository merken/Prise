
using Prise.Infrastructure;
using System;
using System.Collections.Generic;

namespace Prise
{
    public class RemoteTypesProvider : IRemoteTypesProvider
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

        public RemoteTypesProvider AddRemoteType(Type type)
        {
            this.remoteTypes.Add(type);
            return this;
        }

        public RemoteTypesProvider AddRemoteType<T>() => AddRemoteType(typeof(T));

        public IEnumerable<Type> ProvideRemoteTypes() => this.remoteTypes;
    }
}