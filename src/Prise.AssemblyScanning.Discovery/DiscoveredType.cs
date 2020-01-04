#if NETCORE2_1
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Prise.AssemblyScanning.Discovery
{
    [DebuggerDisplay("{Namespace}.{Name}")]
    public class DiscoveredType
    {
        public int Token { get; protected set; }
        public string Name { get; protected set; }
        public string Namespace { get; protected set; }
        public IEnumerable<DiscoveredType> Interfaces { get; protected set; }
        private bool isResolved;
        private bool isLocal;

        private DiscoveredType(int token)
        {
            this.Token = token;
            this.Interfaces = new List<DiscoveredType>();
        }

        private DiscoveredType(int token, string name, string @namespace)
        {
            this.Token = token;
            this.Name = name;
            this.Namespace = @namespace;
            this.Interfaces = new List<DiscoveredType>();
        }

        private DiscoveredType(MetadataReader reader, TypeDefinitionHandle handle)
        {
            var definition = reader.GetTypeDefinition(handle);
            this.Token = reader.GetToken(handle);
            this.Name = reader.GetString(definition.Name);
            this.Namespace = reader.GetString(definition.Namespace);
            this.Interfaces = definition.GetInterfaceImplementations()
                .Select(i => reader.GetInterfaceImplementation(i).Interface)
                .Select(i => DiscoveredType.FromInterface(reader.GetToken(i)));
            this.isLocal = true;
        }


        public static DiscoveredType FromInterface(int token)
            => new DiscoveredType(token);

        public static DiscoveredType FromReferenceType(int token, string name, string @namespace)
            => new DiscoveredType(token, name, @namespace);

        public static DiscoveredType FromDefinedType(MetadataReader reader, TypeDefinitionHandle handle)
            => new DiscoveredType(reader, handle);

        public DiscoveredType Resolve(IEnumerable<DiscoveredType> references)
        {
            if (this.isResolved)
                return this;

            if (this.isLocal)
            {
                var resolvedInterfaces = new List<DiscoveredType>();
                foreach (var @interface in this.Interfaces)
                    resolvedInterfaces.Add(@interface.Resolve(references));
                this.Interfaces = resolvedInterfaces;

                this.isResolved = true;
                return this;
            }

            // Local type resolving
            var resolvedType = references.FirstOrDefault(r => r.Token == this.Token);

            // External type resolving
            if (resolvedType == null)
                resolvedType = references.FirstOrDefault(r =>
                        r.Name.Equals(this.Name, StringComparison.InvariantCulture) &&
                        r.Namespace.Equals(this.Namespace, StringComparison.InvariantCulture));

            return resolvedType;
        }
    }
}
#endif