// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;

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
        private readonly List<string> errors;

        // used for IDataExtensionDependencyTarget
        private readonly HashSet<DataCookerPath> requiredDataCookers = new HashSet<DataCookerPath>();
        private readonly HashSet<DataProcessorId> requiredDataProcessors = new HashSet<DataProcessorId>();

        private DataExtensionDependencyState extensionDependencyState;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type">
        ///     The data extension type.
        /// </param>
        protected DataExtensionReference(Type type) 
            : base(type)
        {
            this.errors = new List<string>();
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="other">
        ///     An existing data extension reference.
        /// </param>
        protected DataExtensionReference(DataExtensionReference<TDerived> other) 
            : base(other)
        {
            this.extensionDependencyState = null;
            if (other.extensionDependencyState != null)
            {
                this.extensionDependencyState = new DataExtensionDependencyState(other.extensionDependencyState);
            }

            this.errors = new List<string>(other.errors);
            this.Errors = new ReadOnlyCollection<string>(this.errors);
        }

        /// <summary>
        ///     A mechanism to identify the data extension when referencing it in messages.
        ///     Defaults to the full name of the Type.
        /// </summary>
        public virtual string Name => this.Type.FullName;

        /// <summary>
        ///     Errors associated with the data extension.
        /// </summary>
        public ReadOnlyCollection<string> Errors { get; }

        /// <summary>
        ///     All of the data cookers that must be available for this extension to be available.
        /// </summary>
        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => this.requiredDataCookers;

        /// <summary>
        ///     All of the data processors that must be available for this extension to be available.
        /// </summary>
        public IReadOnlyCollection<DataProcessorId> RequiredDataProcessors => this.requiredDataProcessors;

        /// <summary>
        ///     The data extension references required by this data extension.
        ///     This is slightly more granular than the <see cref="RequiredDataCookers"/> as it breaks down the cookers
        ///     by source and composite cookers.
        ///     This data only becomes valid after calling <see cref="ProcessDependencies"/>.
        /// </summary>
        public IDataExtensionDependencies DependencyReferences =>
            this.extensionDependencyState?.DependencyReferences ?? null;

        /// <summary>
        ///     This is determined when initializing the data extension from its type. If any errors are encountered, this
        ///     value should be set to Error. Otherwise it should be Available.
        /// </summary>
        public DataExtensionAvailability InitialAvailability { get; protected set; }

        /// <summary>
        ///     If the dependency of this extension has been established, then use that value. Otherwise, falls back to
        ///     <see cref="InitialAvailability"/>.
        /// </summary>
        public DataExtensionAvailability Availability => this.extensionDependencyState?.Availability ?? this.InitialAvailability;

        /// <inheritdoc/>
        public virtual void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataCooker)
        {
        }

        /// <inheritdoc />
        public void ProcessDependencies(IDataExtensionRepository availableDataExtensions)
        {
            Guard.NotNull(availableDataExtensions, nameof(availableDataExtensions));

            if (this.extensionDependencyState == null)
            {
                this.extensionDependencyState = new DataExtensionDependencyState(this);
            }

            this.extensionDependencyState.ProcessDependencies(availableDataExtensions);
        }

        /// <summary>
        ///     Specifies the given
        ///     cooker as required for this extension.
        /// </summary>
        /// <param name="cookerPath">
        ///     The cooker path.
        /// </param>
        protected void AddRequiredDataCooker(DataCookerPath cookerPath)
        {
            this.requiredDataCookers.Add(cookerPath);
        }

        /// <summary>
        ///     Specifies the given
        ///     data processor as required for this extension.
        /// </summary>
        /// <param name="cookerPath">
        ///     The cooker path.
        /// </param>
        protected void AddRequiredDataProcessor(DataProcessorId processorId)
        {
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
        protected void AddError(string error)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(error));

            this.errors.Add(error);
        }
    }
}
