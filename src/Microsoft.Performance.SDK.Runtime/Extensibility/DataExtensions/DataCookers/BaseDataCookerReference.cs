// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataProcessing;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     This class adds data cooker specific functionality on top of the base
    ///     <see cref="DataExtensionReference{TDerived}"/>.
    /// </summary>
    /// <typeparam name="TDerived">
    ///     The class deriving from this type.
    /// </typeparam>
    internal abstract class BaseDataCookerReference<TDerived>
        : DataExtensionReference<TDerived>
          where TDerived : BaseDataCookerReference<TDerived>
    {
        private DataCookerPath path;
        private string description;
        private bool isSourceDataCooker;

        private bool isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseDataCookerReference{TDerived}"/>
        ///     class for the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">
        ///     The <see cref="Type"/> of cooker referenced by this instance.
        ///     It is expected that <see cref="Type"/>is a calid cooker <see cref="Type"/>.
        /// </param>
        protected BaseDataCookerReference(Type type)
            : base(type)
        {
            this.isDisposed = false;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseDataCookerReference{TDerived}"/>
        ///     class as a copy of the given reference.
        /// </summary>
        /// <param name="other">
        ///     The reference from which to create this instance.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     <paramref name="other"/> is disposed.
        /// </exception>
        protected BaseDataCookerReference(DataExtensionReference<TDerived> other)
            : base(other)
        {
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="BaseDataCookerReference{TDerived}"/>
        ///     class.
        /// </summary>
        ~BaseDataCookerReference()
        {
            this.Dispose(false);
        }

        /// <summary>
        ///     Gets the <see cref="DataCookerPath"/> of the referenced
        ///     cooker.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public DataCookerPath Path
        {
            get
            {
                this.ThrowIfDisposed();
                return this.path;
            }
            private set
            {
                this.ThrowIfDisposed();
                this.path = value;
            }
        }

        /// <summary>
        ///     Gets or sets the description of the referenced cooker.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public string Description
        {
            get
            {
                this.ThrowIfDisposed();
                return this.description;
            }
            protected set
            {
                this.ThrowIfDisposed();
                this.description = value;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public override string Name
        {
            get
            {
                this.ThrowIfDisposed();
                return this.Path.ToString();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the referenced
        ///     cooker is a Source Data Cooker.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected bool IsSourceDataCooker
        {
            get
            {
                this.ThrowIfDisposed();
                return this.isSourceDataCooker;
            }
            set
            {
                this.ThrowIfDisposed();
                this.isSourceDataCooker = value;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public override bool Equals(TDerived other)
        {
            return base.Equals(other) &&
                   StringComparer.Ordinal.Equals(this.Path, other.Path) &&
                   StringComparer.Ordinal.Equals(this.Description, other.Description);
        }

        /// <summary>
        ///     When overridden in a derived class, ensures that the
        ///     given instance is considered to describe a valid cooker.
        /// </summary>
        /// <param name="instance">
        ///     The descriptor of the cooker to check.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected virtual void ValidateInstance(IDataCookerDescriptor instance)
        {
            this.ThrowIfDisposed();

            // Create an instance just to pull out the descriptor without saving any references to it.
            if (instance is null)
            {
                this.AddError($"Unable to create an instance of {this.Type}");
                this.InitialAvailability = DataExtensionAvailability.Error;
                return;
            }

            if (string.IsNullOrWhiteSpace(instance.Path.DataCookerId))
            {
                this.AddError("A data cooker Id may not be empty or null.");
                this.InitialAvailability = DataExtensionAvailability.Error;
            }
        }

        /// <summary>
        ///     Initializes the given descriptor with information from the
        ///     cooker reference by this instance.
        /// </summary>
        /// <param name="descriptor">
        ///     The descriptor to initialize.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected void InitializeDescriptorData(IDataCookerDescriptor descriptor)
        {
            Guard.NotNull(descriptor, nameof(descriptor));
            this.ThrowIfDisposed();

            this.Path = descriptor.Path;
            this.Description = descriptor.Description;

            if (this.IsSourceDataCooker)
            {
                if (string.IsNullOrEmpty(descriptor.Path.SourceParserId))
                {
                    this.AddError($"A source data cooker's source Id must not be {nameof(DataCookerPath.EmptySourceParserId)}.");
                    this.InitialAvailability = DataExtensionAvailability.Error;
                }
            }
            else
            {
                if (descriptor.Path.SourceParserId != DataCookerPath.EmptySourceParserId)
                {
                    this.AddError($"A composite data cooker's source Id must be {nameof(DataCookerPath.EmptySourceParserId)}.");
                    this.InitialAvailability = DataExtensionAvailability.Error;
                }
            }

            if (descriptor is IDataCookerDependent cookerRequirements)
            {
                foreach (var dataCookerPath in cookerRequirements.RequiredDataCookers)
                {
                    this.AddRequiredDataCooker(dataCookerPath);
                }
            }

            if (descriptor is IDataProcessorDependent processorRequirements)
            {
                foreach (var dataProcessorId in processorRequirements.RequiredDataProcessors)
                {
                    this.AddRequiredDataProcessor(dataProcessorId);
                }
            }
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
                this.description = null;
                this.path = default;
            }

            this.isDisposed = true;
            base.Dispose(disposing);
        }
    }
}
