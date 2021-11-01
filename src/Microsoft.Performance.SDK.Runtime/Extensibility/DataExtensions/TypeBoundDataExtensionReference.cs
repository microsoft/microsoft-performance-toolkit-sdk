// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     This class wraps <see cref="DataExtensionReference"/> and ties the data extension to a <c>Type</c>.
    /// </summary>
    /// <typeparam name="TDerived">
    ///     The type of class derived from this abstract class.
    /// </typeparam>
    internal abstract class TypeBoundDataExtensionReference<TDerived>
        : AssemblyTypeReference<TDerived>,
          IDataExtensionReference
        where TDerived : TypeBoundDataExtensionReference<TDerived>
    {
        private readonly DataExtensionReference extensionReference;

        private bool isDisposed = false;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="type">
        ///     The data extension type.
        /// </param>
        protected TypeBoundDataExtensionReference(Type type)
            : base(type)
        {
            Debug.Assert(type != null);

            this.extensionReference = new DataExtensionReference(type.FullName);
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="other">
        ///     An existing data extension reference.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     <paramref name="other"/> is disposed.
        /// </exception>
        protected TypeBoundDataExtensionReference(TypeBoundDataExtensionReference<TDerived> other)
            : base(other)
        {
            this.extensionReference = new DataExtensionReference(other.extensionReference);
        }

        /// <summary>
        ///     Gets the name of this extension.
        ///     <para />
        ///     The name is a mechanism to identify the data extension when referencing it 
        ///     in messages. This property defaults to the full name of the Type.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public virtual string Name
        {
            get
            {
                this.ThrowIfDisposed();
                return this.Type.FullName;
            }
        }

        /// <inheritdoc />
        public IDataExtensionDependencyState DependencyState
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionReference.DependencyState;
            }
        }

        /// <inheritdoc />
        public DataExtensionAvailability InitialAvailability
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionReference.InitialAvailability;
            }
            protected set
            {
                this.ThrowIfDisposed();
                this.extensionReference.InitialAvailability = value;
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionReference.RequiredDataCookers;
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<DataProcessorId> RequiredDataProcessors
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionReference.RequiredDataProcessors;
            }
        }

        /// <inheritdoc />
        public DataExtensionAvailability Availability
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionReference.Availability;
            }
        }

        /// <inheritdoc />
        public IDataExtensionDependencies DependencyReferences
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionReference.DependencyReferences;
            }
        }

        /// <inheritdoc />
        public virtual void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataExtension)
        {
            this.ThrowIfDisposed();
            this.extensionReference.PerformAdditionalDataExtensionValidation(
                dependencyStateSupport,
                requiredDataExtension);
        }

        /// <inheritdoc />
        public void ProcessDependencies(IDataExtensionRepository availableDataExtensions)
        {
            this.ThrowIfDisposed();
            this.extensionReference.ProcessDependencies(availableDataExtensions);
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
                this.extensionReference.Dispose();
            }

            this.isDisposed = true;
            base.Dispose(disposing);
        }

        /// <inheritdoc cref="DataExtensionReference.AddRequiredDataCooker(DataCookerPath)"/>
        protected void AddRequiredDataCooker(DataCookerPath cookerPath)
        {
            this.ThrowIfDisposed();
            this.extensionReference.AddRequiredDataCooker(cookerPath);
        }

        /// <inheritdoc cref=DataExtensionReference.AddRequiredDataProcessor(DataProcessorId)"/>
        protected void AddRequiredDataProcessor(DataProcessorId processorId)
        {
            this.ThrowIfDisposed();
            this.extensionReference.AddRequiredDataProcessor(processorId);
        }

        /// <inheritdoc cref=DataExtensionReference.AddError(string)"/>
        protected void AddError(string error)
        {
            this.ThrowIfDisposed();
            this.extensionReference.AddError(error);
        }

        /// <inheritdoc cref=DataExtensionReference.AddError(ErrorInfo)"/>
        protected void AddError(ErrorInfo error)
        {
            this.ThrowIfDisposed();
            this.extensionReference.AddError(error);
        }
    }
}
