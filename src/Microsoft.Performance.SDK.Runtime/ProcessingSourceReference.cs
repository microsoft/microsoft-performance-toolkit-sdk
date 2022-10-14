// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Creates a <see cref="AssemblyTypeReferenceWithInstance{T, Derived}"/> where T is <see cref="IProcessingSource"/> and Derived is <see cref="ProcessingSourceReference"/>.
    /// </summary>
    public sealed class ProcessingSourceReference
        : AssemblyTypeReferenceWithInstance<IProcessingSource, ProcessingSourceReference>
    {
        private ReadOnlyHashSet<DataSourceAttribute> dataSourceAttributes;

        private Guid guid;
        private string name;
        private string description;
        private ReadOnlyCollection<Option> commandLineOptionsRO;

        private List<ICustomDataProcessor> createdProcessors;
        private ReadOnlyCollection<ICustomDataProcessor> createdProcessorsRO;

        private bool isInitialized;
        private bool isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceReference"/>
        ///     class.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> of instance being referenced by this instance.
        /// </param>
        /// <param name="metadata">
        ///     The <see cref="IProcessingSource"/> metadata.
        /// </param>
        /// <param name="dataSourceAttributes">
        ///     The declared data sources.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="metadata"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="dataSourceAttributes"/> is <c>null</c>.
        /// </exception>
        public ProcessingSourceReference(
            Type type,
            ProcessingSourceAttribute metadata,
            HashSet<DataSourceAttribute> dataSourceAttributes)
            : base(type)
        {
            Guard.NotNull(metadata, nameof(metadata));
            Guard.NotNull(dataSourceAttributes, nameof(dataSourceAttributes));

            this.InitializeThis(metadata, dataSourceAttributes);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceReference"/>
        ///     class.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> of instance being referenced by this instance.
        /// </param>
        /// <param name="instanceFactory">
        ///     The factory function for creating the instance.
        /// </param>
        /// <param name="metadata">
        ///     The <see cref="IProcessingSource"/> metadata.
        /// </param>
        /// <param name="dataSourceAttributes">
        ///     The declared data sources.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="type"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="instanceFactory"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="metadata"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="dataSourceAttributes"/> is <c>null</c>.
        /// </exception>
        public ProcessingSourceReference(
            Type type,
            Func<IProcessingSource> instanceFactory,
            ProcessingSourceAttribute metadata,
            HashSet<DataSourceAttribute> dataSourceAttributes)
            : base(type, instanceFactory)
        {
            Guard.NotNull(metadata, nameof(metadata));
            Guard.NotNull(dataSourceAttributes, nameof(dataSourceAttributes));

            this.InitializeThis(metadata, dataSourceAttributes);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceReference"/>
        ///     class with the given instance.
        /// </summary>
        /// <param name="instance">
        ///     The existing <see cref="IProcessingSource"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="instance"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="instance"/> does not have a <see cref="ProcessingSourceAttribute"/>.
        /// </exception>
        public ProcessingSourceReference(IProcessingSource instance)
            : base(instance.GetType(), () => instance)
        {
            Guard.NotNull(instance, nameof(instance));

            var metadata = instance.GetType().GetCustomAttribute<ProcessingSourceAttribute>();
            if (metadata is null)
            {
                throw new ArgumentException($"{nameof(ProcessingSourceAttribute)} is missing from type {instance.GetType()}", nameof(instance));
            }

            var dataSourceAttributes = instance.GetType()
                .GetCustomAttributes<DataSourceAttribute>()
                .ToSet();

            this.InitializeThis(metadata, dataSourceAttributes);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessingSourceReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///     The instance from which to make a copy.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="other"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        ///     <paramref name="other"/> is disposed.
        /// </exception>
        public ProcessingSourceReference(ProcessingSourceReference other)
            : base(other.Type)
        {
            other.ThrowIfDisposed();

            this.dataSourceAttributes = other.dataSourceAttributes;

            this.guid = other.guid;
            this.name = other.name;
            this.description = other.description;
            this.commandLineOptionsRO = other.commandLineOptionsRO;
            this.createdProcessors = other.createdProcessors;
            this.createdProcessorsRO = other.createdProcessorsRO;

            this.isInitialized = true;
            this.isDisposed = false;
        }

        /// <inheritdoc cref="ProcessingSourceAttribute.Guid"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public Guid Guid
        {
            get
            {
                this.EnsureUsable();
                return this.guid;
            }
        }

        /// <inheritdoc cref="ProcessingSourceAttribute.Name"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public string Name
        {
            get
            {
                this.EnsureUsable();
                return this.name;
            }
        }

        /// <inheritdoc cref="ProcessingSourceAttribute.Description"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public string Description
        {
            get
            {
                this.EnsureUsable();
                return this.description;
            }
        }

        /// <summary>
        ///     Gets the <see cref="DataSourceAttribute"/>s for the <see cref="IProcessingSource"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyCollection<DataSourceAttribute> DataSources
        {
            get
            {
                this.EnsureUsable();
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
                this.EnsureUsable();
                return this.Instance.DataTables.Union(this.Instance.MetadataTables).ToList().AsReadOnly();
            }
        }

        /// <inheritdoc cref="IProcessingSource.CommandLineOptions"/>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<Option> CommandLineOptions
        {
            get
            {
                this.EnsureUsable();
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
                this.EnsureUsable();
                return this.createdProcessorsRO;
            }
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="ProcessingSourceReference"/> 
        ///     based on the <paramref name="candidateType"/>.
        ///     <para/>
        ///     A <see cref="Type"/> must satisfy the following criteria in order to 
        ///     be eligible as a reference:
        ///     <list type="bullet">
        ///         <item>must be public.</item>
        ///         <item>must be concrete.</item>
        ///         <item>must implement <see cref="IProcessingSource"/> somewhere in the inheritance hierarchy (either directly or indirectly.)</item>
        ///         <item>must have a public parameterless constructor.</item>
        ///         <item>must be decorated with the <see cref="ProcessingSourceAttribute"/>.</item>
        ///         <item>must be decorated with one (1) or more <see cref="DataSourceAttribute"/>s.</item>
        ///     </list>
        /// </summary>
        /// <param name="candidateType">
        ///     Candidate <see cref="Type"/> for the <see cref="ProcessingSourceReference"/>
        /// </param>
        /// <param name="reference">
        ///     Out <see cref="ProcessingSourceReference"/>
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of <see cref="ProcessingSourceReference"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool TryCreateReference(
            Type candidateType,
            out ProcessingSourceReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;
            if (IsValidType(candidateType))
            {
                if (candidateType.TryGetEmptyPublicConstructor(out var constructor))
                {
                    var metadataAttribute = candidateType.GetCustomAttribute<ProcessingSourceAttribute>();
                    if (metadataAttribute != null)
                    {
                        var dataSourceAttributes = candidateType.GetCustomAttributes<DataSourceAttribute>()?.ToList();
                        if (dataSourceAttributes != null &&
                            dataSourceAttributes.Count > 0)
                        {
                            reference = new ProcessingSourceReference(
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
        ///     Creates a new processor for processing the specified <see cref="IDataSourceGroup"/>.
        /// </summary>
        /// <param name="dataSourceGroup">
        ///     The data source group to process.
        /// </param>
        /// <param name="processorEnvironment">
        ///     The environment for this specific processor instance.
        /// </param>
        /// <param name="commandLineOptions">
        ///     The command line options to pass to the processor.
        /// </param>
        /// <returns>
        ///     The created <see cref="ICustomDataProcessor"/>.
        /// </returns>
        public ICustomDataProcessor CreateProcessor(
            IDataSourceGroup dataSourceGroup,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions commandLineOptions)
        {
            this.EnsureUsable();

            ICustomDataProcessor processor = null;
            try
            {
                processor = this.Instance.CreateProcessor(
                    dataSourceGroup,
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
        ///     is supported by the <see cref="IProcessingSource>"/> referenced by
        ///     this <see cref="ProcessingSourceReference"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The <see cref="IDataSource"/> to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="dataSource"/> is supported by 
        ///     the <see cref="IProcessingSource"/> referenced by this instance; <c>false</c>
        ///     otherwise.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool Supports(IDataSource dataSource)
        {
            this.EnsureUsable();

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
        public override ProcessingSourceReference CloneT()
        {
            this.EnsureUsable();

            return new ProcessingSourceReference(this);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            this.EnsureUsable();

            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as ProcessingSourceReference;

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
            this.EnsureUsable();

            return HashCodeUtils.CombineHashCodeValues(
                base.GetHashCode(),
                this.Name.GetHashCode(),
                this.Guid.GetHashCode(),
                this.Description.GetHashCode(),
                this.DataSources.GetHashCode());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            this.EnsureUsable();

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

        [Conditional("DEBUG")]
        private void AssertInitialized()
        {
            Debug.Assert(this.isInitialized);
        }

        private void EnsureUsable()
        {
            this.AssertInitialized();
            this.ThrowIfDisposed();
        }

        private void InitializeThis(
            ProcessingSourceAttribute metadata,
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

            this.isInitialized = true;
            this.isDisposed = false;
        }
    }
}
