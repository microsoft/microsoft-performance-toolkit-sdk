// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Creates a <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> where T is <see cref="ICustomDataSource"/> and Derived is <see cref="CustomDataSourceReference"/>.
    /// </summary>
    public sealed class CustomDataSourceReference
        : AssemblyTypeReferenceWithInstance<ICustomDataSource, CustomDataSourceReference>
    {
        private static CustomDataSourceReferenceComparer equalityComparer;

        private CustomDataSourceReference(
            Type type,
            CustomDataSourceAttribute metadata,
            DataSourceAttribute dataSource)
            : base(type)
        {
            Debug.Assert(metadata != null);
            Debug.Assert(dataSource != null);

            Debug.Assert(metadata.Equals(type.GetCustomAttribute<CustomDataSourceAttribute>()));
            Debug.Assert(dataSource.Equals(type.GetCustomAttribute<DataSourceAttribute>()));

            this.Guid = metadata.Guid;
            this.Name = metadata.Name;
            this.Description = metadata.Description;
            this.DataSource = dataSource;
            this.CommandLineOptions = this.Instance.CommandLineOptions.ToList().AsReadOnly();
        }

        private CustomDataSourceReference(
            CustomDataSourceReference other)
            : base(other)
        {
            this.Guid = other.Guid;
            this.Name = other.Name;
            this.Description = other.Description;
            this.DataSource = other.DataSource;
            this.CommandLineOptions = other.CommandLineOptions;
        }

        /// <inheritdoc cref="CustomDataSourceAttribute.Guid"/>
        public Guid Guid { get; }

        /// <inheritdoc cref="CustomDataSourceAttribute.Name"/>
        public string Name { get; }

        /// <inheritdoc cref="CustomDataSourceAttribute.Description"/>
        public string Description { get; }

        /// <summary>
        ///     Gets the <see cref="DataSourceAttribute"/> for the custom data source.
        /// </summary>
        public DataSourceAttribute DataSource { get; }

        /// <inheritdoc cref="ICustomDataSource.DataTables"/>
        public IEnumerable<TableDescriptor> AvailableTables => this.Instance.DataTables.ToList().AsReadOnly();

        /// <inheritdoc cref="ICustomDataSource.CommandLineOptions"/>
        public IEnumerable<Option> CommandLineOptions { get; }

        /// <summary>
        ///     Tries to create a instance of the <see cref="CustomDataSourceReference"/> based on the <paramref name="candidateType"/>
        /// </summary>
        /// <param name="candidateType">
        ///     Candidate <see cref="Type"/> for the <see cref="CustomDataSourceReference"/>
        /// </param>
        /// <param name="reference">
        ///     Out <see cref="CustomDataSourceReference"/>
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of <see cref="CustomDataSourceReference"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreateReference(
            Type candidateType,
            out CustomDataSourceReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;
            if (IsValidType(candidateType))
            {
                if (candidateType.TryGetEmptyPublicConstructor(out var constructor))
                {
                    var metadataAttribute = candidateType.GetCustomAttribute<CustomDataSourceAttribute>();
                    if (metadataAttribute != null)
                    {
                        var dataSourceAttribute = candidateType.GetCustomAttribute<DataSourceAttribute>();
                        if (dataSourceAttribute != null)
                        {
                            reference = new CustomDataSourceReference(
                                candidateType,
                                metadataAttribute,
                                dataSourceAttribute);
                        }
                    }
                }
            }

            return reference != null;
        }

        public override string ToString()
        {
            return $"{this.Name} - {this.Guid} ({this.AssemblyPath})";
        }

        public override bool Equals(CustomDataSourceReference other)
        {
            return base.Equals(other) &&
                this.Name.Equals(other.Name) &&
                this.Guid.Equals(other.Guid) &&
                this.Description.Equals(other.Description) &&
                this.DataSource.Equals(other.DataSource);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = base.GetHashCode();

                hash = ((hash << 5) + hash) ^ this.Name.GetHashCode();
                hash = ((hash << 5) + hash) ^ this.Guid.GetHashCode();
                hash = ((hash << 5) + hash) ^ this.Description.GetHashCode();
                hash = ((hash << 5) + hash) ^ this.DataSource.GetHashCode();

                return hash;
            }
        }

        public override CustomDataSourceReference CloneT()
        {
            return new CustomDataSourceReference(this);
        }

        /// <summary>
        ///     Gets the default <see cref="IEqualityComparer{T}"/> for <see cref="CustomDataSourceReference"/>.
        /// </summary>
        public static IEqualityComparer<CustomDataSourceReference> EqualityComparer
        {
            get
            {
                if (equalityComparer == null)
                {
                    equalityComparer = new CustomDataSourceReferenceComparer();
                }

                return equalityComparer;
            }
        }

        private sealed class CustomDataSourceReferenceComparer
            : IEqualityComparer<CustomDataSourceReference>
        {
            public bool Equals(CustomDataSourceReference c1, CustomDataSourceReference c2)
            {
                return c1.Equals(c2);
            }

            public int GetHashCode(CustomDataSourceReference c)
            {
                return (c != null) ? c.GetHashCode() : 0;
            }
        }
    }
}
