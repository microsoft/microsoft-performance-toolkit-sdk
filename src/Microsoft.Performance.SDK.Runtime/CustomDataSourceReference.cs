// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Creates a <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> where T is <see cref="ICustomDataSource"/> and Derived is <see cref="CustomDataSourceReference"/>.
    /// </summary>
    public sealed class CustomDataSourceReference
        : AssemblyTypeReferenceWithInstance<ICustomDataSource, CustomDataSourceReference>
    {
        private ReadOnlyHashSet<DataSourceAttribute> dataSourceAttributes;

        private Guid guid;
        private string name;
        private string description;
        private ReadOnlyCollection<Option> commandLineOptionsRO;

        private List<ICustomDataProcessor> createdProcessors;
        private ReadOnlyCollection<ICustomDataProcessor> createdProcessorsRO;

        private bool isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceReference"/>
        ///     class.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> of instance being referenced by this instance.
        /// </param>
        /// <param name="metadata">
        ///     The CustomDataSource metadata.
        /// </param>
        /// <param name="dataSourceAttributes">
        ///     The declared data sources.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        /// </exception>
        public CustomDataSourceReference(
            Type type,
            CustomDataSourceAttribute metadata,
            HashSet<DataSourceAttribute> dataSourceAttributes)
            : base(type)
        {
            this.InitializeThis(metadata, dataSourceAttributes);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceReference"/>
        ///     class.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> of instance being referenced by this instance.
        /// </param>
        /// <param name="instanceFactory">
        ///     The factory function for creating the instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        /// </exception>
        public CustomDataSourceReference(
            Type type,
            Func<ICustomDataSource> instanceFactory,
            CustomDataSourceAttribute metadata,
            HashSet<DataSourceAttribute> dataSourceAttributes)
            : base(type, instanceFactory)
        {
            this.InitializeThis(metadata, dataSourceAttributes);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceReference"/>
        ///     class with the given instance.
        /// </summary>
        /// <param name="instance">
        ///     The existing <see cref="ICustomDataSource"/>.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="instance"/> does not have a <see cref="CustomDataSourceAttribute"/>.
        /// </exception>
        public CustomDataSourceReference(ICustomDataSource instance)
            : base(instance.GetType(), () => instance)
        {
            var metadata = instance.GetType().GetCustomAttribute<CustomDataSourceAttribute>();
            if (metadata is null)
            {
                throw new ArgumentException($"{nameof(CustomDataSourceAttribute)} is missing from type {instance.GetType()}", nameof(instance));
            }

            var dataSourceAttributes = instance.GetType()
                .GetCustomAttributes<DataSourceAttribute>()
                .ToSet();

            this.InitializeThis(metadata, dataSourceAttributes);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomDataSourceReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///     The instance from which to make a copy.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     <paramref name="other"/> is disposed.
        /// </exception>
        public CustomDataSourceReference(CustomDataSourceReference other)
            : base(other.Type)
        {
            this.dataSourceAttributes = other.dataSourceAttributes;

            this.guid = other.guid;
            this.name = other.name;
            this.description = other.description;
            this.commandLineOptionsRO = other.commandLineOptionsRO;

            this.isDisposed = other.isDisposed;
        }

        /// <inheritdoc cref="CustomDataSourceAttribute.Guid"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public Guid Guid
        {
            get
            {
                this.ThrowIfDisposed();
                return this.guid;
            }
        }

        /// <inheritdoc cref="CustomDataSourceAttribute.Name"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public string Name
        {
            get
            {
                this.ThrowIfDisposed();
                return this.name;
            }
        }

        /// <inheritdoc cref="CustomDataSourceAttribute.Description"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public string Description
        {
            get
            {
                this.ThrowIfDisposed();
                return this.description;
            }
        }

        /// <summary>
        ///     Gets the <see cref="DataSourceAttribute"/>s for the custom data source.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyCollection<DataSourceAttribute> DataSources
        {
            get
            {
                this.ThrowIfDisposed();
                return this.dataSourceAttributes;
            }
        }

        /// <summary>
        ///     Gets the collection of tables exposed by this data source.
        ///     These are both metadata and data tables for exposing the processed data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<TableDescriptor> AvailableTables
        {
            get
            {
                this.ThrowIfDisposed();
                return this.Instance.DataTables.Union(this.Instance.MetadataTables).ToList().AsReadOnly();
            }
        }

        /// <inheritdoc cref="ICustomDataSource.CommandLineOptions"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<Option> CommandLineOptions
        {
            get
            {
                this.ThrowIfDisposed();
                return this.commandLineOptionsRO;
            }
        }

        /// <summary>
        ///     Gets a collection of all processors that have been created
        ///     by this reference.
        /// </summary>        
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<ICustomDataProcessor> TrackedProcessors
        {
            get
            {
                this.ThrowIfDisposed();
                return this.createdProcessorsRO;
            }
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="CustomDataSourceReference"/> 
        ///     based on the <paramref name="candidateType"/>.
        ///     <para/>
        ///     A <see cref="Type"/> must satisfy the following criteria in order to 
        ///     be eligible as a reference:
        ///     <list type="bullet">
        ///         <item>must be public.</item>
        ///         <item>must be concrete.</item>
        ///         <item>must implement ICustomDataSource somewhere in the inheritance heirarchy (either directly or indirectly.)</item>
        ///         <item>must have a public parameterless constructor.</item>
        ///         <item>must be decorated with the <see cref="CustomDataSourceAttribute"/>.</item>
        ///         <item>must be decorated with one (1) or more <see cref="DataSourceAttribute"/>s.</item>
        ///     </list>
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
                            reference = new CustomDataSourceReference(
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
        ///     Creates a new processor for processing the specified data sources.
        /// </summary>
        /// <param name="dataSources">
        ///     The data sources to process.
        /// </param>
        /// <param name="processorEnvironment">
        ///     The environment for this specific processor instance.
        /// </param>
        /// <param name="options">
        ///     The command line options to pass to the processor.
        /// </param>
        /// <returns>
        ///     The created <see cref="ICustomDataProcessor"/>.
        /// </returns>
        public ICustomDataProcessor CreateProcessor(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions commandLineOptions)
        {
            this.ThrowIfDisposed();

            ICustomDataProcessor processor = null;
            try
            {
                processor = this.Instance.CreateProcessor(
                    dataSources,
                    processorEnvironment,
                    commandLineOptions);
                this.createdProcessors.Add(processor);
                return processor;
            }
            catch
            {
                try
                {
                    this.Instance.DisposeProcessor(processor);
                }
                finally
                {
                    processor.TryDispose();
                }

                throw;
            }
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
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool Supports(IDataSource dataSource)
        {
            this.ThrowIfDisposed();
            if (dataSource is null)
            {
                return false;
            }

            try
            {
                return this.DataSources.Where(x => dataSource.GetType().Is(x.Type))
                                       .Any(x => x.Accepts(dataSource)) &&
                       this.Instance.IsDataSourceSupported(dataSource);
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override CustomDataSourceReference CloneT()
        {
            this.ThrowIfDisposed();
            return new CustomDataSourceReference(this);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            this.ThrowIfDisposed();
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as CustomDataSourceReference;

            return other != null &&
                this.Name.Equals(other.Name) &&
                this.Guid.Equals(other.Guid) &&
                this.Description.Equals(other.Description) &&
                (this.dataSourceAttributes.IsSubsetOf(other.dataSourceAttributes) &&
                 other.dataSourceAttributes.IsSubsetOf(this.dataSourceAttributes));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            this.ThrowIfDisposed();
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

        /// <inheritdoc />
        public override string ToString()
        {
            this.ThrowIfDisposed();
            return $"{this.Name} - {this.Guid} ({this.AssemblyPath})";
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var p in this.createdProcessors)
                {
                    try
                    {
                        this.Instance.DisposeProcessor(p);
                    }
                    catch
                    {
                    }

                    p.TryDispose();
                }

                this.createdProcessors = null;
                this.commandLineOptionsRO = null;
                this.dataSourceAttributes = null;
                this.description = null;
                this.guid = default;
                this.name = null;
            }

            base.Dispose(disposing);
            this.isDisposed = true;
        }

        private void InitializeThis(
            CustomDataSourceAttribute metadata,
            HashSet<DataSourceAttribute> dataSourceAttributes)
        {
            Debug.Assert(metadata != null);
            Debug.Assert(dataSourceAttributes != null);

            this.dataSourceAttributes = new ReadOnlyHashSet<DataSourceAttribute>(dataSourceAttributes);

            this.guid = metadata.Guid;
            this.name = metadata.Name;
            this.description = metadata.Description;
            this.commandLineOptionsRO = this.Instance.CommandLineOptions.ToList().AsReadOnly();
            this.createdProcessors = new List<ICustomDataProcessor>();
            this.createdProcessorsRO = new ReadOnlyCollection<ICustomDataProcessor>(this.createdProcessors);

            this.isDisposed = false;
        }
    }
}
