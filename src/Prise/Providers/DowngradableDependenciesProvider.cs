using System;
using System.Collections.Generic;
using Prise.Infrastructure;

namespace Prise.Providers
{
    public class DowngradableDependenciesProvider<T> : IDowngradableDependenciesProvider<T>
    {
        private IList<Type> types;
        private IList<string> assemblies;
        protected bool disposed = false;
        public DowngradableDependenciesProvider()
        {
            this.types = new List<Type>();
            this.assemblies = new List<string>();
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

        public DowngradableDependenciesProvider<T> AddDowngradableType(Type type)
        {
            this.types.Add(type);
            return this;
        }

        public DowngradableDependenciesProvider<T> AddDowngradableAssembly(string assembly)
        {
            this.assemblies.Add(assembly);
            return this;
        }

        public DowngradableDependenciesProvider<T> AddDowngradableType<THostType>() => AddDowngradableType(typeof(THostType));

        public IEnumerable<Type> ProvideDowngradableTypes() => this.types;
        public IEnumerable<string> ProvideDowngradableAssemblies() => this.assemblies;
    }
}
