// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     A data extension reference is a wrapper around a type that is a data extension.
    ///     This class provides functionality for determining availability by examining availability of required data
    ///     extensions.
    /// </summary>
    /// <typeparam name="TDerived">
    ///     The type of class derived from this abstract class.
    /// </typeparam>
    internal abstract class DataExtensionReference<TDerived>
        : AssemblyTypeReference<TDerived>,
          IDataExtensionReference
        where TDerived : DataExtensionReference<TDerived>
    {
        private List<ErrorInfo> errors;
        private ReadOnlyCollection<ErrorInfo> errorsRO;

        // used for IDataExtensionDependencyTarget
        private HashSet<DataCookerPath> requiredDataCookers = new HashSet<DataCookerPath>();
        private HashSet<DataProcessorId> requiredDataProcessors = new HashSet<DataProcessorId>();

        private DataExtensionDependencyState extensionDependencyState;
        private DataExtensionAvailability initializeAvailability;

        private bool isDisposed = false;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type">
        ///     The data extension type.
        /// </param>
        protected DataExtensionReference(Type type)
            : base(type)
        {
            this.errors = new List<ErrorInfo>();
            this.errorsRO = new ReadOnlyCollection<ErrorInfo>(this.errors);
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
        protected DataExtensionReference(DataExtensionReference<TDerived> other)
            : base(other)
        {
            this.extensionDependencyState = null;
            if (other.extensionDependencyState != null)
            {
                this.extensionDependencyState = new DataExtensionDependencyState(other.extensionDependencyState);
            }

            this.errors = new List<ErrorInfo>(other.errors);
            this.errorsRO = new ReadOnlyCollection<ErrorInfo>(this.errors);
        }

        /// <inheritdoc />
        public IDataExtensionDependencyState DependencyState
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionDependencyState;
            }
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

        /// <summary>
        ///     Gets all of the data cookers that must be available for this extension to be available.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers
        {
            get
            {
                this.ThrowIfDisposed();
                return this.requiredDataCookers;
            }
        }

        /// <summary>
        ///     Gets all of the data processors that must be available for this extension to be available.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyCollection<DataProcessorId> RequiredDataProcessors
        {
            get
            {
                this.ThrowIfDisposed();
                return this.requiredDataProcessors;
            }
        }

        /// <summary>
        ///     Gets the data extension references required by this data extension.
        ///     This is slightly more granular than the <see cref="RequiredDataCookers"/> as it breaks down the cookers
        ///     by source and composite cookers.
        ///     This data only becomes valid after calling <see cref="ProcessDependencies"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataExtensionDependencies DependencyReferences
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionDependencyState?.DependencyReferences ?? null;
            }
        }

        /// <summary>
        ///     Gets the value indicating when the referenced instance is available.
        ///     <para />
        ///     This is determined when initializing the data extension from its type. If any errors are encountered, this
        ///     value should be set to Error. Otherwise it should be Available.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public DataExtensionAvailability InitialAvailability
        {
            get
            {
                this.ThrowIfDisposed();
                return this.initializeAvailability;
            }
            protected set
            {
                this.ThrowIfDisposed();
                this.initializeAvailability = value;
            }
        }

        /// <summary>
        ///     Gets the current availability of the referened instance.
        ///     <para />
        ///     If the dependency of this extension has been established, then use that value. Otherwise, falls back to
        ///     <see cref="InitialAvailability"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public DataExtensionAvailability Availability
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionDependencyState?.Availability ?? this.InitialAvailability;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public virtual void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataCooker)
        {
            Guard.NotNull(dependencyStateSupport, nameof(dependencyStateSupport));
            Guard.NotNull(requiredDataCooker, nameof(requiredDataCooker));
            this.ThrowIfDisposed();
        }

        /// <inheritdoc />
        public void ProcessDependencies(IDataExtensionRepository availableDataExtensions)
        {
            Guard.NotNull(availableDataExtensions, nameof(availableDataExtensions));
            this.ThrowIfDisposed();

            if (this.extensionDependencyState == null)
            {
                this.extensionDependencyState = new DataExtensionDependencyState(this);
            }

            this.extensionDependencyState.ProcessDependencies(availableDataExtensions);
        }

        /// <inheritdoc />
        public void Release()
        {
            this.ThrowIfDisposed();
            this.ReleaseCore();
        }

        /// <summary>
        ///     When overridden in a derived class, releases
        ///     any instances tracked by this reference.
        /// </summary>
        protected virtual void ReleaseCore()
        {
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
                this.errors = null;
                this.errorsRO = null;
                this.requiredDataCookers = null;
                this.requiredDataProcessors = null;
                this.extensionDependencyState = null;
            }

            this.isDisposed = true;
            base.Dispose(disposing);
        }

        /// <summary>
        ///     Specifies the given
        ///     cooker as required for this extension.
        /// </summary>
        /// <param name="cookerPath">
        ///     The cooker path.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected void AddRequiredDataCooker(DataCookerPath cookerPath)
        {
            this.ThrowIfDisposed();
            this.requiredDataCookers.Add(cookerPath);
        }

        /// <summary>
        ///     Specifies the given
        ///     data processor as required for this extension.
        /// </summary>
        /// <param name="cookerPath">
        ///     The cooker path.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected void AddRequiredDataProcessor(DataProcessorId processorId)
        {
            this.ThrowIfDisposed();
            this.requiredDataProcessors.Add(processorId);
        }

        /// <summary>
        ///     Adds an error to this instance. This is used to keep
        ///     track of all errors seen during processing so that they
        ///     can be meaningfully presented to the user.
        /// </summary>
        /// <param name="error">
        ///     The error that occurred.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected void AddError(string error)
        {
            this.AddError(
                new ErrorInfo(
                    ErrorCodes.EXTENSION_Error,
                    ErrorCodes.EXTENSION_Error.Description)
                {
                    Target = this.Name,
                    Details = new[]
                    {
                        new ErrorInfo(ErrorCodes.EXTENSION_Error,  error),
                    },
                });
        }

        /// <summary>
        ///     Adds an error to this instance. This is used to keep
        ///     track of all errors seen during processing so that they
        ///     can be meaningfully presented to the user.
        /// </summary>
        /// <param name="error">
        ///     The error that occurred.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected void AddError(ErrorInfo error)
        {
            Debug.Assert(error != null);

            this.ThrowIfDisposed();
            this.errors.Add(error);
        }
    }
}
