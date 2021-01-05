// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Resolves the Custom Data Sources that can process given Data Sources.
    /// </summary>
    public static class DataSourceResolver
    {
        /// <summary>
        ///     Given a collection of <see cref="IDataSource"/>s and
        ///     <see cref="CustomDataSourceReference"/>s, creates a mapping
        ///     that assigns each Data Source to one or more Custom Data Sources
        ///     on the basis of what can be processed by each Custom Data Source.
        ///     <para />
        ///     Each Custom Data Source in <paramref name="customDataSources"/> will be a
        ///     key in the returned mapping. The value for any given key represents
        ///     the  Data Sources that can be processed the given Custom Data Source. If
        ///     no Data Sources can be processed by a given Custom Data Source, then the
        ///     value will be an empty collection.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to be mappedto the Custom Data Sources that are able
        ///     to process them. Any duplicate values in this parameter will be
        ///     ignored. Any null elements in this parameter will be ignored.
        /// </param>
        /// <param name="customDataSources">
        ///     The collection of Custom Data Source references that are potentially
        ///     eligible to process the elements of <paramref name="dataSources"/>.
        ///     Any duplicate values in this parameter will be ignored. Any null elements
        ///     in this parameter will be ignored.
        /// </param>
        /// <returns>
        ///     A mapping of Custom Data Sources to collections of zero (0) or more
        ///     Data Sources that can be processed by said Custom Data Sources.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="customDataSources"/> is <c>null</c>.
        /// </exception>
        public static IDictionary<CustomDataSourceReference, IEnumerable<IDataSource>> Assign(
            IEnumerable<IDataSource> dataSources,
            IEnumerable<CustomDataSourceReference> customDataSources)
        {
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(customDataSources, nameof(customDataSources));

            var dedupedDs = new HashSet<IDataSource>(dataSources.Where(x => x is object));
            var dedupedCds = new HashSet<CustomDataSourceReference>(customDataSources.Where(x => x is object));

            var dataSourceFilters = dedupedDs.ToDictionary(
                x => x,
                GenerateCdsFilter);

            var assignment = new Dictionary<CustomDataSourceReference, IEnumerable<IDataSource>>();

            foreach (var cds in dedupedCds)
            {
                Debug.Assert(cds != null);

                var cdsDataSources = new List<IDataSource>();
                foreach (var ds in dedupedDs)
                {
                    Debug.Assert(ds != null);
                    Debug.Assert(dataSourceFilters.ContainsKey(ds));

                    var filter = dataSourceFilters[ds];
                    Debug.Assert(filter != null);

                    if (filter(cds) && cds.Supports(ds))
                    {
                        cdsDataSources.Add(ds);
                    }
                }

                Debug.Assert(!assignment.ContainsKey(cds));
                assignment[cds] = cdsDataSources;
            }

            Debug.Assert(customDataSources.All(assignment.ContainsKey));

            return assignment;
        }

        private static Func<CustomDataSourceReference, bool> GenerateCdsFilter(
            IDataSource dataSource)
        {
            Debug.Assert(dataSource != null);

            switch (dataSource)
            {
                case DirectoryDataSource _:
                    {
                        return x => x.AreDirectoriesSupported();
                    }

                case FileDataSource fds:
                    {
                        var extension = FileExtensionUtils.GetCanonicalExtension(fds.FullPath);
                        if (string.IsNullOrWhiteSpace(extension))
                        {
                            return x => x.AreExtensionlessFilesSupported();
                        }

                        Debug.Assert(extension != null);

                        return x => x.TryGetCanonicalFileExtensions().Contains(extension);
                    }

                default:
                    {
                        return _ => true;
                    }
            }
        }
    }
}
