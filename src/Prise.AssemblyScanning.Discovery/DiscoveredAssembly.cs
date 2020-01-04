#if NETCORE2_1
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Prise.AssemblyScanning.Discovery
{
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
#endif