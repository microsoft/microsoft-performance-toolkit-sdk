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
    public abstract class CustomDataSourceReference
        : AssemblyTypeReferenceWithInstance<ICustomDataSource, CustomDataSourceReference>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceReference"/>
        ///     class.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> of instance being referenced by this instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        /// </exception>
        protected CustomDataSourceReference(Type type)
            : base(type)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///     The instance from which to make a copy.
        /// </param>
        protected CustomDataSourceReference(CustomDataSourceReference other)
            : base(other.Type)
        {
        }

        /// <inheritdoc cref="CustomDataSourceAttribute.Guid"/>
        public virtual Guid Guid { get; }

        /// <inheritdoc cref="CustomDataSourceAttribute.Name"/>
        public virtual string Name { get; }

        /// <inheritdoc cref="CustomDataSourceAttribute.Description"/>
        public virtual string Description { get; }

        /// <summary>
        ///     Gets the <see cref="DataSourceAttribute"/>s for the custom data source.
        /// </summary>
        public virtual IReadOnlyCollection<DataSourceAttribute> DataSources { get; }

        /// <inheritdoc cref="ICustomDataSource.DataTables"/>
        public virtual IEnumerable<TableDescriptor> AvailableTables { get; }

        /// <inheritdoc cref="ICustomDataSource.CommandLineOptions"/>
        public virtual IEnumerable<Option> CommandLineOptions { get; }

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
                        var dataSourceAttributes = candidateType.GetCustomAttributes<DataSourceAttribute>()?.ToList();
                        if (dataSourceAttributes != null &&
                            dataSourceAttributes.Count > 0)
                        {
                            reference = new CustomDataSourceReferenceImpl(
                                candidateType,
                                metadataAttribute,
                                new HashSet<DataSourceAttribute>(dataSourceAttributes));
                        }
                    }
                }
            }

            return reference != null;
        }

        /// <summary>
        ///     Determines whether the given <see cref="IDataSource"/>
        ///     is supported by the <see cref="ICustomDataSource>"/> referenced by
        ///     this <see cref="CustomDataSourceReference"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The <see cref="IDataSource"/> to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="dataSource"/> is supported by 
        ///     the Custom Data Source referenced by this instance; <c>false</c>
        ///     otherwise.
        /// </returns>
        public abstract bool Supports(IDataSource dataSource);

        private sealed class CustomDataSourceReferenceImpl
            : CustomDataSourceReference
        {
            private readonly ReadOnlyHashSet<DataSourceAttribute> dataSourceAttributes;

            internal CustomDataSourceReferenceImpl(
                Type type,
                CustomDataSourceAttribute metadata,
                HashSet<DataSourceAttribute> dataSourceAttributes)
                : base(type)
            {
                Debug.Assert(metadata != null);
                Debug.Assert(dataSourceAttributes != null);

                Debug.Assert(metadata.Equals(type.GetCustomAttribute<CustomDataSourceAttribute>()));

                this.dataSourceAttributes = new ReadOnlyHashSet<DataSourceAttribute>(dataSourceAttributes);

                this.Guid = metadata.Guid;
                this.Name = metadata.Name;
                this.Description = metadata.Description;
                this.CommandLineOptions = this.Instance.CommandLineOptions.ToList().AsReadOnly();
            }

            internal CustomDataSourceReferenceImpl(
                CustomDataSourceReferenceImpl other)
                : base(other)
            {
                this.dataSourceAttributes = other.dataSourceAttributes;

                this.Guid = other.Guid;
                this.Name = other.Name;
                this.Description = other.Description;
                this.CommandLineOptions = other.CommandLineOptions;
            }

            public override Guid Guid { get; }

            public override string Name { get; }

            public override string Description { get; }

            public override IReadOnlyCollection<DataSourceAttribute> DataSources => this.dataSourceAttributes;

            public override IEnumerable<TableDescriptor> AvailableTables => this.Instance.DataTables.ToList().AsReadOnly();

            public override IEnumerable<Option> CommandLineOptions { get; }

            public override bool Supports(IDataSource dataSource)
            {
                if (dataSource is null)
                {
                    return false;
                }

                try
                {
                    return this.Instance.IsDataSourceSupported(dataSource);
                }
                catch
                {
                    return false;
                }
            }

            public override CustomDataSourceReference CloneT()
            {
                return new CustomDataSourceReferenceImpl(this);
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as CustomDataSourceReferenceImpl;

                return other != null &&
                    this.Name.Equals(other.Name) &&
                    this.Guid.Equals(other.Guid) &&
                    this.Description.Equals(other.Description) &&
                    (this.dataSourceAttributes.IsSubsetOf(other.dataSourceAttributes) &&
                     other.dataSourceAttributes.IsSubsetOf(this.dataSourceAttributes));
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = base.GetHashCode();

                    hash = ((hash << 5) + hash) ^ this.Name.GetHashCode();
                    hash = ((hash << 5) + hash) ^ this.Guid.GetHashCode();
                    hash = ((hash << 5) + hash) ^ this.Description.GetHashCode();
                    hash = ((hash << 5) + hash) ^ this.DataSources.GetHashCode();

                    return hash;
                }
            }

            public override string ToString()
            {
                return $"{this.Name} - {this.Guid} ({this.AssemblyPath})";
            }
        }
    }
}
