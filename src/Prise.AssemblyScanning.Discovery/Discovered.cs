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

    [DebuggerDisplay("{Name}")]
    public class DiscoveredAssembly
    {
        public int Token { get; protected set; }
        public string Name { get; protected set; }
        public string Path { get; protected set; }
        public IEnumerable<DiscoveredAssembly> ReferencedAssemblies { get; protected set; }
        public IEnumerable<DiscoveredType> DefinedTypes { get; protected set; }
        public IEnumerable<DiscoveredType> ReferencedTypes { get; protected set; }
        public IEnumerable<DiscoveredType> ResolvedTypes { get; protected set; }

        private DiscoveredAssembly(int token)
        {
            this.Token = token;
        }

        private DiscoveredAssembly(int token, string name)
        {
            this.Token = token;
            this.Name = name;
        }

        public static DiscoveredAssembly FromReference(int token, string name)
            => new DiscoveredAssembly(token, name);

        public static DiscoveredAssembly Unresolved(int token)
            => new DiscoveredAssembly(token);

        public DiscoveredAssembly(string path, MetadataReader reader)
        {
            var definition = reader.GetAssemblyDefinition();

            this.Path = path;

            this.Name = reader.GetString(definition.Name);

            this.ReferencedAssemblies = reader.AssemblyReferences
                .Select(r => new { Token = reader.GetToken(r), AssemblyReference = reader.GetAssemblyReference(r) })
                .Select(tr => DiscoveredAssembly.FromReference(tr.Token, reader.GetString(tr.AssemblyReference.Name)));

            this.DefinedTypes = reader.TypeDefinitions
                     .Select(t => DiscoveredType.FromDefinedType(reader, t));

            this.ReferencedTypes = reader.TypeReferences
                    .Select(r => new { Token = reader.GetToken(r), TypeReference = reader.GetTypeReference(r) })
                    .Select(tr =>
                        DiscoveredType.FromReferenceType(
                        tr.Token,
                        reader.GetString(tr.TypeReference.Name),
                        reader.GetString(tr.TypeReference.Namespace)));

            var allTypes = this.DefinedTypes.Union(this.ReferencedTypes);
            var resolvedTypes = new List<DiscoveredType>();
            foreach (var type in allTypes)
                resolvedTypes.Add(type.Resolve(allTypes));
            this.ResolvedTypes = resolvedTypes;
        }

        public DiscoveredAssembly Resolve(IEnumerable<DiscoveredAssembly> references)
        {
            var allTypesFromAssembly = this.DefinedTypes.Union(this.ReferencedTypes);

            foreach (var type in this.DefinedTypes)
                type.Resolve(allTypesFromAssembly);

            foreach (var type in this.ReferencedTypes)
                type.Resolve(allTypesFromAssembly);

            return this;
        }
    }
}
