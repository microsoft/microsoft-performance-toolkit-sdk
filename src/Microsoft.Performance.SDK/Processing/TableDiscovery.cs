// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides common <see cref="IProcessingSourceTableProvider"/> implementations.
    /// </summary>
    public static class TableDiscovery
    {
        /// <summary>
        ///     Creates a new <see cref="IProcessingSourceTableProvider"/> instance that will
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
        ///     A new <see cref="IProcessingSourceTableProvider"/> that discovers tables in the
        ///     <see cref="Assembly"/> containing the <paramref name="processingSource"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="processingSource"/> is <c>null</c>.
        /// </exception>
        public static IProcessingSourceTableProvider CreateForAssembly(IProcessingSource processingSource)
        {
            Guard.NotNull(processingSource, nameof(processingSource));

            return CreateForAssembly(processingSource.GetType().Assembly);
        }

        /// <summary>
        ///     Creates a new <see cref="IProcessingSourceTableProvider"/> instance that will
        ///     discover tables in the given <see cref="Assembly"/>. Tables are
        ///     discovered in the assembly by being a type decorated with the
        ///     <see cref="TableAttribute"/> attribute.
        /// </summary>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> to search for tables.
        /// </param>
        /// <returns>
        ///     A new <see cref="IProcessingSourceTableProvider"/> that discovers tables in the
        ///     specified <paramref name="assembly"/>.
        /// </returns>
        public static IProcessingSourceTableProvider CreateForAssembly(Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            return new AssemblyTableDiscoverer(assembly);
        }

        /// <summary>
        ///     Creates a new <see cref="IProcessingSourceTableProvider"/> instance that will
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
        ///     A new <see cref="IProcessingSourceTableProvider"/> that discovers tables in the
        ///     specified <paramref name="tableNamespace"/> in the <see cref="Assembly"/>
        ///     containing <paramref name="processingSource"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="processingSource"/> is <c>null</c>.
        /// </exception>
        public static IProcessingSourceTableProvider CreateForNamespace(string tableNamespace, IProcessingSource processingSource)
        {
            Guard.NotNull(processingSource, nameof(processingSource));

            return CreateForNamespace(tableNamespace, processingSource.GetType().Assembly);
        }

        /// <summary>
        ///     Creates a new <see cref="IProcessingSourceTableProvider"/> instance that will
        ///     discover tables in the given namespace in the given assembly.
        ///     <para/>
        ///     No table that requires <see cref="IDataCooker"/>s or implements a static
        ///     <c>BuildTable&lt;<see cref="ITableBuilder"/>, <see cref="IDataExtensionRetrieval"/>&gt;</c>
        ///     will be returned from this <see cref="IProcessingSourceTableProvider"/>.
        /// </summary>
        /// <param name="tableNamespace">
        ///     The namespace to search for tables. This parameter may be <c>null</c> to
        ///     specify the global namespace.
        /// </param>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> to search for tables.
        /// </param>
        /// <returns>
        ///     A new <see cref="IProcessingSourceTableProvider"/> that discovers tables in the
        ///     specified <paramref name="tableNamespace"/> in the specified <paramref name="assembly"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="assembly"/> is <c>null</c>.
        /// </exception>
        public static IProcessingSourceTableProvider CreateForNamespace(string tableNamespace, Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            return CreateForNamespace(tableNamespace, new[] { assembly, });
        }

        /// <summary>
        ///     Creates a new <see cref="IProcessingSourceTableProvider"/> instance that will
        ///     discover tables in the given namespace in the given assemblies.
        ///     <para/>
        ///     No table that requires <see cref="IDataCooker"/>s or implements a static
        ///     <c>BuildTable&lt;<see cref="ITableBuilder"/>, <see cref="IDataExtensionRetrieval"/>&gt;</c>
        ///     will be returned from this <see cref="IProcessingSourceTableProvider"/>.
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
        ///     A new <see cref="IProcessingSourceTableProvider"/> that discovers tables in the
        ///     specified <paramref name="tableNamespace"/> in the specified <paramref name="assemblies"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="assemblies"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="assemblies"/> has no non-null elements.
        /// </exception>
        public static IProcessingSourceTableProvider CreateForNamespace(string tableNamespace, Assembly[] assemblies)
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

        private static IEnumerable<TableDescriptor> DoAssemblyDiscovery(
            IEnumerable<Assembly> assemblies,
            Func<Type, bool> typeFilter,
            ITableConfigurationsSerializer tableConfigSerializer)
        {
            Debug.Assert(assemblies != null);
            Debug.Assert(typeFilter != null);
            Debug.Assert(assemblies.Any());

            var s = new HashSet<TableDescriptor>();
            foreach (var t in assemblies.SelectMany(x => x.GetTypes()).Where(typeFilter))
            {
                if (TableDescriptorFactory.TryCreate(
                    t,
                    tableConfigSerializer,
                    out TableDescriptor td,
                    out Action<ITableBuilder, Extensibility.IDataExtensionRetrieval> buildTable))
                {
                    if (!td.RequiresDataExtensions() && buildTable == null)
                    {
                        s.Add(td);
                    }
                }
            }

            return s;
        }

        private sealed class AssemblyTableDiscoverer
            : IProcessingSourceTableProvider
        {
            private readonly Assembly assembly;

            internal AssemblyTableDiscoverer(Assembly assembly)
            {
                Debug.Assert(assembly != null);

                this.assembly = assembly;
            }

            public IEnumerable<TableDescriptor> Discover(ITableConfigurationsSerializer tableConfigSerializer)
            {
                return DoAssemblyDiscovery(new[] { assembly, }, _ => true, tableConfigSerializer);
            }
        }

        private sealed class NamespaceTableDiscoverer
            : IProcessingSourceTableProvider
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

            public IEnumerable<TableDescriptor> Discover(ITableConfigurationsSerializer tableConfigSerializer)
            {
                return DoAssemblyDiscovery(
                    this.assemblies,
                    x => StringComparer.Ordinal.Equals(this.tableNamespace, x.Namespace),
                    tableConfigSerializer);
            }
        }
    }
}
