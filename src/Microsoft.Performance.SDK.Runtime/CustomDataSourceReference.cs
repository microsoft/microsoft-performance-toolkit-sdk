// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public abstract Guid Guid { get; }

        /// <inheritdoc cref="CustomDataSourceAttribute.Name"/>
        public abstract string Name { get; }

        /// <inheritdoc cref="CustomDataSourceAttribute.Description"/>
        public abstract string Description { get; }

        /// <summary>
        ///     Gets the <see cref="DataSourceAttribute"/>s for the custom data source.
        /// </summary>
        public abstract IReadOnlyCollection<DataSourceAttribute> DataSources { get; }

        /// <inheritdoc cref="ICustomDataSource.DataTables"/>
        public abstract IEnumerable<TableDescriptor> AvailableTables { get; }

        /// <inheritdoc cref="ICustomDataSource.CommandLineOptions"/>
        public abstract IEnumerable<Option> CommandLineOptions { get; }

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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private sealed class CustomDataSourceReferenceImpl
            : CustomDataSourceReference
        {
            private readonly ReadOnlyHashSet<DataSourceAttribute> dataSourceAttributes;

            private readonly Guid guid;
            private readonly string name;
            private readonly string description;
            private readonly ReadOnlyCollection<Option> commandLineOptionsRO;

            private bool isDisposed;

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

                this.guid = metadata.Guid;
                this.name = metadata.Name;
                this.description = metadata.Description;
                this.commandLineOptionsRO = this.Instance.CommandLineOptions.ToList().AsReadOnly();
            }

            internal CustomDataSourceReferenceImpl(
                CustomDataSourceReferenceImpl other)
                : base(other)
            {
                this.dataSourceAttributes = other.dataSourceAttributes;

                this.guid = other.guid;
                this.name = other.name;
                this.description = other.description;
                this.commandLineOptionsRO = other.commandLineOptionsRO;
            }

            public override Guid Guid
            {
                get
                {
                    this.ThrowIfDisposed();
                    return this.guid;
                }
            }

            public override string Name
            {
                get
                {
                    this.ThrowIfDisposed();
                    return this.name;
                }
            }

            public override string Description
            {
                get
                {
                    this.ThrowIfDisposed();
                    return this.description;
                }
            }

            public override IReadOnlyCollection<DataSourceAttribute> DataSources
            {
                get
                {
                    this.ThrowIfDisposed();
                    return this.dataSourceAttributes;
                }
            }

            public override IEnumerable<TableDescriptor> AvailableTables
            {
                get
                {
                    this.ThrowIfDisposed();
                    return this.Instance.DataTables.ToList().AsReadOnly();
                }
            }

            public override IEnumerable<Option> CommandLineOptions
            {
                get
                {
                    this.ThrowIfDisposed();
                    return this.commandLineOptionsRO;
                }
            }

            public override bool Supports(IDataSource dataSource)
            {
                this.ThrowIfDisposed();
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
                this.ThrowIfDisposed();
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

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (this.isDisposed)
                {
                    return;
                }

                if (disposing)
                {
                }

                this.isDisposed = true;
            }
        }
    }
}
