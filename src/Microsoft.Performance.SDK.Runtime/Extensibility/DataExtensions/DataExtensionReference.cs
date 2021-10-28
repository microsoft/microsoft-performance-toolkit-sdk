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
    internal class DataExtensionReference
        : IDataExtensionReference
    {
        private readonly string name;

        private List<ErrorInfo> errors;
        private ReadOnlyCollection<ErrorInfo> errorsRO;

        // used for IDataExtensionDependencyTarget
        private HashSet<DataCookerPath> requiredDataCookers = new HashSet<DataCookerPath>();
        private HashSet<DataProcessorId> requiredDataProcessors = new HashSet<DataProcessorId>();

        private DataExtensionDependencyState extensionDependencyState;
        private DataExtensionAvailability initializeAvailability;

        private bool isDisposed = false;

        /// <summary>
        ///     Initializes an instance of this class.
        /// </summary>
        /// <param name="name">
        ///     A display name for the data extension.
        /// </param>
        internal DataExtensionReference(string name)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(name));

            this.errors = new List<ErrorInfo>();
            this.errorsRO = new ReadOnlyCollection<ErrorInfo>(this.errors);
            this.name = name;
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
        internal DataExtensionReference(DataExtensionReference other)
        {
            this.extensionDependencyState = null;
            if (other.extensionDependencyState != null)
            {
                this.extensionDependencyState = new DataExtensionDependencyState(other.extensionDependencyState);
            }

            this.errors = new List<ErrorInfo>(other.errors);
            this.errorsRO = new ReadOnlyCollection<ErrorInfo>(this.errors);
            this.name = other.name;
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
        ///     in messages.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public virtual string Name
        {
            get
            {
                this.ThrowIfDisposed();
                return this.name;
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
            // todo: it seems like this is only ever set to or compared to DataExtensionAvailability.Error
            //       given this, should it just be made into a bool?

            get
            {
                this.ThrowIfDisposed();
                return this.initializeAvailability;
            }
            internal set
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
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // nothing here to dispose
            }

            this.errors = null;
            this.errorsRO = null;
            this.requiredDataCookers = null;
            this.requiredDataProcessors = null;
            this.extensionDependencyState = null;

            this.isDisposed = true;
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
        internal void AddRequiredDataCooker(DataCookerPath cookerPath)
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
        internal void AddRequiredDataProcessor(DataProcessorId processorId)
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
        internal void AddError(string error)
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
        internal void AddError(ErrorInfo error)
        {
            Debug.Assert(error != null);

            this.ThrowIfDisposed();
            this.errors.Add(error);
        }

        /// <summary>
        ///     Throws an exception if this instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}
