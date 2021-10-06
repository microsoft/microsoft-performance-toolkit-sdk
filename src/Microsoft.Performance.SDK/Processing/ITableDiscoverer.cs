// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides an interface for providing tables that are exposed
    ///     by <see cref="ProcessingSource"/>s.
    /// </summary>
    public interface ITableProvider
    {
        /// <summary>
        ///     Returns the collection of tables that should be associated with a Processing Source.
        /// </summary>
        /// <param name="tableConfigSerializer">
        ///     The serializer used to deserialize table configurations.
        /// </param>
        /// <returns>
        ///     A collection of tables.
        /// </returns>
        ISet<DiscoveredTable> Discover(ISerializer tableConfigSerializer);
    }

    /// <summary>
    ///     Provides common <see cref="ITableProvider"/> implementations.
    /// </summary>
    public static class TableDiscovery
    {
        /// <summary>
        ///     Creates a new <see cref="ITableProvider"/> instance that will
        ///     discover tables in the given <see cref="Assembly"/>. Tables are
        ///     discovered in the assembly by being a type decorated with the
        ///     <see cref="TableAttribute"/> attribute.
        /// </summary>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> to search for tables.
        /// </param>
        /// <returns>
        ///     A new <see cref="ITableProvider"/> that discovers tables in the
        ///     specified <paramref name="assembly"/>.
        /// </returns>
        public static ITableProvider CreateForAssembly(Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            return new AssemblyTableDiscoverer(assembly);
        }

        /// <summary>
        ///     Creates a new <see cref="ITableProvider"/> instance that will
        ///     discover tables in the given namespace in the given assembly.
        /// </summary>
        /// <param name="namespace">
        ///     The namespace to search for tables. This parameter may be empty to
        ///     specify the global namespace.
        /// </param>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> to search for tables.
        /// </param>
        /// <returns>
        ///     A new <see cref="ITableProvider"/> that discovers tables in the
        ///     specified <paramref name="namespace"/> in the specified <paramref name="assembly"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="namespace"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="assembly"/> is <c>null</c>.
        /// </exception>
        public static ITableProvider CreateForNamespace(string @namespace, Assembly assembly)
        {
            Guard.NotNull(@namespace, nameof(@namespace));
            Guard.NotNull(assembly, nameof(assembly));

            return CreateForNamespace(@namespace, new[] { assembly, });
        }

        /// <summary>
        ///     Creates a new <see cref="ITableProvider"/> instance that will
        ///     discover tables in the given namespace in the given assembly.
        /// </summary>
        /// <param name="namespace">
        ///     The namespace to search for tables.
        /// </param>
        /// <param name="assemblies">
        ///     The <see cref="Assembly"/> instances to search for tables. Any null elements in
        ///     this parameter are ignored. Any duplicate assemblies are ignored.
        /// </param>
        /// <returns>
        ///     A new <see cref="ITableProvider"/> that discovers tables in the
        ///     specified <paramref name="namespace"/> in the specified <paramref name="assembly"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="namespace"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="assemblies"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="assemblies"/> has no non-null elements.
        /// </exception>
        public static ITableProvider CreateForNamespace(string @namespace, Assembly[] assemblies)
        {
            Guard.NotNull(@namespace, nameof(@namespace));
            Guard.NotNull(assemblies, nameof(assemblies));

            var set = new HashSet<Assembly>();

            for (var i = 0; i < assemblies.Length; ++i)
            {
                if (assemblies[i] != null)
                {
                    set.Add(assemblies[i]);
                }
            }

            if (set.Count == 0)
            {
                throw new ArgumentException(
                    $"{nameof(assemblies)} must contain at least one non-null element.",
                    nameof(assemblies));
            }

            return new NamespaceTableDiscoverer(@namespace, set);
        }

        private static ISet<DiscoveredTable> DoAssemblyDiscovery(
            IEnumerable<Assembly> assemblies,
            Func<Type, bool> typeFilter,
            ISerializer tableConfigSerializer)
        {
            Debug.Assert(assemblies != null);
            Debug.Assert(typeFilter != null);
            Debug.Assert(assemblies.Any());

            var s = new HashSet<DiscoveredTable>();
            foreach (var t in assemblies.SelectMany(x => x.GetTypes()).Where(typeFilter))
            {
                if (TableDescriptorFactory.TryCreate(t, tableConfigSerializer, out var isInternal, out var td, out var buildTable))
                {
                    s.Add(new DiscoveredTable(td, buildTable, isInternal));
                }
            }

            return s;
        }

        private sealed class AssemblyTableDiscoverer
            : ITableProvider
        {
            private readonly Assembly assembly;

            internal AssemblyTableDiscoverer(Assembly assembly)
            {
                Debug.Assert(assembly != null);

                this.assembly = assembly;
            }

            public ISet<DiscoveredTable> Discover(ISerializer tableConfigSerializer)
            {
                return DoAssemblyDiscovery(new[] { assembly, }, _ => true, tableConfigSerializer);
            }
        }

        private sealed class NamespaceTableDiscoverer
            : ITableProvider
        {
            private readonly string @namespace;
            private readonly HashSet<Assembly> assemblies;

            internal NamespaceTableDiscoverer(string @namespace, HashSet<Assembly> assemblies)
            {
                Debug.Assert(@namespace != null);
                Debug.Assert(assemblies != null);
                Debug.Assert(assemblies.Count > 0);
                Debug.Assert(assemblies.All(x => x != null));

                this.@namespace = @namespace;
                this.assemblies = assemblies;
            }

            public ISet<DiscoveredTable> Discover(ISerializer tableConfigSerializer)
            {
                return DoAssemblyDiscovery(
                    this.assemblies,
                    x => StringComparer.Ordinal.Equals(this.@namespace, x.Namespace),
                    tableConfigSerializer);
            }
        }
    }

    /// <summary>
    ///     Represents a table discovered by an <see cref="ITableProvider"/>
    ///     instance.
    /// </summary>
    public sealed class DiscoveredTable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscoveredTable"/>
        ///     class, indicating that the table with the given <see cref="TableDescriptor"/>
        ///     exists, and uses the default build action.
        /// </summary>
        /// <param name="descriptor">
        ///     The descriptor of the table.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="descriptor"/> is <c>null</c>.
        /// </exception>
        public DiscoveredTable(TableDescriptor descriptor)
            : this(descriptor, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscoveredTable"/>
        ///     class, indicating that the table with the given <see cref="TableDescriptor"/>
        ///     exists, and is built via the specified build action.
        /// </summary>
        /// <param name="descriptor">
        ///     The descriptor of the table.
        /// </param>
        /// <param name="buildTable">
        ///     The build action for the table. This parameter may be <c>null</c> to specify
        ///     that the default build action should be used.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="descriptor"/> is <c>null</c>.
        /// </exception>
        public DiscoveredTable(TableDescriptor descriptor, Action<ITableBuilder, IDataExtensionRetrieval> buildTable)
            : this(descriptor, buildTable, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscoveredTable"/>
        ///     class, indicating that the table with the given <see cref="TableDescriptor"/>
        ///     exists, and is built via the specified build action.
        /// </summary>
        /// <param name="descriptor">
        ///     The descriptor of the table.
        /// </param>
        /// <param name="buildTable">
        ///     The build action for the table. This parameter may be <c>null</c> to specify
        ///     that the default build action should be used.
        /// </param>
        /// <param name="isInternal">
        ///     A value indicating whether this table is internal to the plugin.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="descriptor"/> is <c>null</c>.
        /// </exception>
        public DiscoveredTable(
            TableDescriptor descriptor, 
            Action<ITableBuilder, IDataExtensionRetrieval> buildTable,
            bool isInternal)
        {
            Guard.NotNull(descriptor, nameof(descriptor));

            this.Descriptor = descriptor;
            this.BuildTable = buildTable;
            this.IsInternal = isInternal;
        }

        /// <summary>
        ///     Gets the <see cref="TableDescriptor"/> of the table.
        /// </summary>
        public TableDescriptor Descriptor { get; }

        /// <summary>
        ///     Gets the build action of the table. This property may be <c>null</c>.
        /// </summary>
        public Action<ITableBuilder, IDataExtensionRetrieval> BuildTable { get; }

        /// <summary>
        ///     Gets a value indicating whether this table is internal to the plugin.
        /// </summary>
        public bool IsInternal { get; }

        /// <summary>
        ///     Gets a value indicating whether this table is a metadata table.
        /// </summary>
        public bool IsMetadata => this.Descriptor.IsMetadataTable;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as DiscoveredTable;
            return other != null &&
                   this.Descriptor.Equals(other.Descriptor);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Descriptor.GetHashCode();
        }
    }
}
