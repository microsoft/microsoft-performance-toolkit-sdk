// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides common <see cref="ITableProvider"/> implementations.
    /// </summary>
    public static class TableDiscovery
    {
        /// <summary>
        ///     Creates a new <see cref="ITableProvider"/> instance that will
        ///     discover tables in the <see cref="Assembly"/> containing the
        ///     given <see cref="IProcessingSource"/>. Tables are discovered in the
        ///     assembly by being a type decorated with the <see cref="TableAttribute"/>
        ///     attribute.
        /// </summary>
        /// <param name="processingSource">
        ///     The <see cref="IProcessingSource"/> whose <see cref="Assembly"/> is to
        ///     be searched for tables.
        /// </param>
        /// <returns>
        ///     A new <see cref="ITableProvider"/> that discovers tables in the
        ///     <see cref="Assembly"/> containing the <paramref name="processingSource"/>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="processingSource"/> is <c>null</c>.
        /// </exception>
        public static ITableProvider CreateForAssembly(IProcessingSource processingSource)
        {
            Guard.NotNull(processingSource, nameof(processingSource));

            return CreateForAssembly(processingSource.GetType().Assembly);
        }

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
        ///     discover tables in the given namespace in the <see cref="Assembly"/>
        ///     containing the specified <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="tableNamespace">
        ///     The namespace to search for tables. This parameter may be <c>null</c> to
        ///     specify the global namespace.
        /// </param>
        /// <param name="processingSource">
        ///     The <see cref="IProcessingSource"/> whose <see cref="Assembly"/> is
        ///     to be searched for tables.
        /// </param>
        /// <returns>
        ///     A new <see cref="ITableProvider"/> that discovers tables in the
        ///     specified <paramref name="tableNamespace"/> in the <see cref="Assembly"/>
        ///     containing <paramref name="processingSource"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="processingSource"/> is <c>null</c>.
        /// </exception>
        public static ITableProvider CreateForNamespace(string tableNamespace, IProcessingSource processingSource)
        {
            Guard.NotNull(processingSource, nameof(processingSource));

            return CreateForNamespace(tableNamespace, processingSource.GetType().Assembly);
        }

        /// <summary>
        ///     Creates a new <see cref="ITableProvider"/> instance that will
        ///     discover tables in the given namespace in the given assembly.
        /// </summary>
        /// <param name="tableNamespace">
        ///     The namespace to search for tables. This parameter may be <c>null</c> to
        ///     specify the global namespace.
        /// </param>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> to search for tables.
        /// </param>
        /// <returns>
        ///     A new <see cref="ITableProvider"/> that discovers tables in the
        ///     specified <paramref name="tableNamespace"/> in the specified <paramref name="assembly"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="assembly"/> is <c>null</c>.
        /// </exception>
        public static ITableProvider CreateForNamespace(string tableNamespace, Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            return CreateForNamespace(tableNamespace, new[] { assembly, });
        }

        /// <summary>
        ///     Creates a new <see cref="ITableProvider"/> instance that will
        ///     discover tables in the given namespace in the given assemblies.
        /// </summary>
        /// <param name="tableNamespace">
        ///     The namespace to search for tables. This parameter may be <c>null</c> to
        ///     specify the global namespace.
        /// </param>
        /// <param name="assemblies">
        ///     The <see cref="Assembly"/> instances to search for tables. Any null elements in
        ///     this parameter are ignored. Any duplicate assemblies are ignored.
        /// </param>
        /// <returns>
        ///     A new <see cref="ITableProvider"/> that discovers tables in the
        ///     specified <paramref name="tableNamespace"/> in the specified <paramref name="assembly"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="assemblies"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="assemblies"/> has no non-null elements.
        /// </exception>
        public static ITableProvider CreateForNamespace(string tableNamespace, Assembly[] assemblies)
        {
            Guard.NotNull(assemblies, nameof(assemblies));

            var set = new HashSet<Assembly>(assemblies.Where(x => !(x is null)));
            if (set.Count == 0)
            {
                throw new ArgumentException(
                    $"{nameof(assemblies)} must contain at least one non-null element.",
                    nameof(assemblies));
            }

            return new NamespaceTableDiscoverer(tableNamespace, set);
        }

        private static IEnumerable<DiscoveredTable> DoAssemblyDiscovery(
            IEnumerable<Assembly> assemblies,
            Func<Type, bool> typeFilter,
            ISerializer tableConfigSerializer)
        {
            Debug.Assert(assemblies != null);
            Debug.Assert(typeFilter != null);
            Debug.Assert(assemblies.Any());

            var s = new HashSet<DiscoveredTable>(DiscoveredTableEqualityComparer.ByTableDescriptor);
            foreach (var t in assemblies.SelectMany(x => x.GetTypes()).Where(typeFilter))
            {
                if (TableDescriptorFactory.TryCreate(
                    t,
                    tableConfigSerializer,
                    out TableDescriptor td,
                    out Action<ITableBuilder, Extensibility.IDataExtensionRetrieval> buildTable))
                {
                    s.Add(new DiscoveredTable(td, buildTable));
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

            public IEnumerable<DiscoveredTable> Discover(ISerializer tableConfigSerializer)
            {
                return DoAssemblyDiscovery(new[] { assembly, }, _ => true, tableConfigSerializer);
            }
        }

        private sealed class NamespaceTableDiscoverer
            : ITableProvider
        {
            private readonly string tableNamespace;
            private readonly HashSet<Assembly> assemblies;

            internal NamespaceTableDiscoverer(string tableNamespace, HashSet<Assembly> assemblies)
            {
                Debug.Assert(assemblies != null);
                Debug.Assert(assemblies.Count > 0);
                Debug.Assert(assemblies.All(x => x != null));

                this.tableNamespace = tableNamespace;
                this.assemblies = assemblies;
            }

            public IEnumerable<DiscoveredTable> Discover(ISerializer tableConfigSerializer)
            {
                return DoAssemblyDiscovery(
                    this.assemblies,
                    x => StringComparer.Ordinal.Equals(this.tableNamespace, x.Namespace),
                    tableConfigSerializer);
            }
        }
    }
}
