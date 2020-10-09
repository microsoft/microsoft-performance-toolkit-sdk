// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    /// <summary>
    ///     This class builds a set of dependencies for a target data extension object.
    ///     This may be any type of extension: data cooker, data processor, table... as
    ///     long as it implements IDataExtensionDependencyTarget.
    ///
    ///     The base class <see cref="DataExtensionReference&lt;TDerived&gt;"/> implements
    ///     IDataExtensionDependencyTarget.
    ///
    ///     Maintains all data associated with dependencies for the target, including
    ///     status, error messages, and missing dependencies - as well as the data extension
    ///     references that make up the dependencies.
    /// </summary>
    internal class DataExtensionDependencyState
        : IDataExtensionDependency,
          IDataExtensionDependencyStateSupport,
          ICloneable
    {
        private enum EstablishAvailabilityStatus
        {
            NotStarted,
            InProgress,
            Complete
        }

        private readonly List<string> errors;
        private readonly HashSet<DataProcessorId> missingDataProcessors;
        private readonly HashSet<DataCookerPath> missingDataCookers;

        private EstablishAvailabilityStatus establishAvailabilityStatus = EstablishAvailabilityStatus.NotStarted;

        private readonly IDataExtensionDependencyTarget target;

        private readonly DataExtensionDependencies dependencyReferences;

        public DataExtensionDependencyState(IDataExtensionDependencyTarget dataDependencyTarget)
        {
            Guard.NotNull(dataDependencyTarget, nameof(dataDependencyTarget));

            this.errors = new List<string>();
            this.Errors = new ReadOnlyCollection<string>(this.errors);

            this.missingDataCookers = new HashSet<DataCookerPath>();

            this.missingDataProcessors = new HashSet<DataProcessorId>();

            this.target = dataDependencyTarget;

            this.dependencyReferences = new DataExtensionDependencies();
        }

        public DataExtensionDependencyState(DataExtensionDependencyState other)
        {
            Guard.NotNull(other, nameof(other));

            this.target = other.target;

            this.errors = new List<string>(other.errors);
            this.Errors = new ReadOnlyCollection<string>(this.errors);

            this.missingDataCookers = new HashSet<DataCookerPath>(other.missingDataCookers);
            this.missingDataProcessors = new HashSet<DataProcessorId>(other.missingDataProcessors);

            this.Availability = other.Availability;

            this.dependencyReferences = new DataExtensionDependencies(other.dependencyReferences);
        }

        /// <summary>
        ///     Gets the availability status. If not <see cref="DataExtensionAvailability.Available"/>, then
        ///     the data extension may not be used.
        /// </summary>
        public DataExtensionAvailability Availability { get; private set; }

        /// <summary>
        ///     Gets error messages reported while establishing dependencies.
        /// </summary>
        public IReadOnlyCollection<string> Errors { get; }

        /// <summary>
        ///     Gets data cookers that are required but unavailable.
        /// </summary>
        public IReadOnlyCollection<DataCookerPath> MissingDataCookers => this.missingDataCookers;

        /// <summary>
        /// Gets data processors that are required but unavailable.
        /// </summary>
        public IReadOnlyCollection<DataProcessorId> MissingDataProcessors => this.missingDataProcessors;

        /// <summary>
        ///     An object that exposes all the data extension references that are available
        ///     and required by the data extension target of this dependency data.
        /// </summary>
        public IDataExtensionDependencies DependencyReferences => this.dependencyReferences;

        /// <summary>
        ///     This is a wrapper around <see cref="ValidateDataCookerDependencies"/>. It performs some prechecks
        ///     before iterating over all of the dependencies. It also performs meta-checks, looking for errors
        ///     like circular dependencies.
        /// </summary>
        /// <param name="availableDataExtensions">
        ///     Data extensions that have been exposed through the runtime.
        /// </param>
        public void ProcessDependencies(IDataExtensionRepository availableDataExtensions)
        {
            Guard.NotNull(availableDataExtensions, nameof(availableDataExtensions));

            if (this.establishAvailabilityStatus == EstablishAvailabilityStatus.InProgress)
            {
                this.AddError($"There is a circular dependency between data extensions: {this.target.Name}.");
                this.UpdateAvailability(DataExtensionAvailability.Error);
                return;
            }

            if (this.establishAvailabilityStatus == EstablishAvailabilityStatus.Complete)
            {
                return;
            }

            if (this.establishAvailabilityStatus == EstablishAvailabilityStatus.InProgress)
            {
                this.errors.Add($"There is a circular dependency between data extensions involving {this.target.Name}");
                this.UpdateAvailability(DataExtensionAvailability.Error);
                return;
            }

            this.establishAvailabilityStatus = EstablishAvailabilityStatus.InProgress;

            this.UpdateAvailability(this.target.InitialAvailability);

            if (this.Availability == DataExtensionAvailability.Error)
            {
                // this means there was an error in the target, so just stop now without any further
                // validation.
                //
                this.establishAvailabilityStatus = EstablishAvailabilityStatus.Complete;
                return;
            }

            this.ValidateDataCookerDependencies(availableDataExtensions);

            this.ValidateRequiredDataProcessors(availableDataExtensions);

            this.UpdateAvailability(DataExtensionAvailability.Available);

            this.establishAvailabilityStatus = EstablishAvailabilityStatus.Complete;
        }

        public object Clone()
        {
            return new DataExtensionDependencyState(this);
        }

        /// <summary>
        ///     This establishes and maintains a priority for availability status values.
        ///     Precedence is:
        ///     <see cref="DataExtensionAvailability.Undetermined"/>
        ///         Passing in  causes a reset.
        ///     <see cref="DataExtensionAvailability.Error"/> 
        ///     <see cref="DataExtensionAvailability.IndirectError"/> 
        ///     <see cref="DataExtensionAvailability.MissingRequirement"/> 
        ///     <see cref="DataExtensionAvailability.MissingIndirectRequirement"/> 
        ///     <see cref="DataExtensionAvailability.Available"/> 
        /// </summary>
        /// <param name="availability">
        ///     An availability state based on some validation rule.
        /// </param>
        public void UpdateAvailability(DataExtensionAvailability availability)
        {
            // In case of a reset, always allow undetermined to be set.
            if (availability == DataExtensionAvailability.Undetermined)
            {
                this.ClearMessages();
                this.Availability = availability;
                return;
            }

            if (availability == DataExtensionAvailability.Error)
            {
                this.Availability = availability;
            }

            if (this.Availability == DataExtensionAvailability.Error)
            {
                return;
            }

            if (availability == DataExtensionAvailability.IndirectError)
            {
                this.Availability = availability;
            }

            if (this.Availability == DataExtensionAvailability.IndirectError)
            {
                return;
            }

            if (availability == DataExtensionAvailability.MissingRequirement)
            {
                this.Availability = DataExtensionAvailability.MissingRequirement;
            }

            if (this.Availability == DataExtensionAvailability.MissingRequirement)
            {
                return;
            }

            if (availability == DataExtensionAvailability.MissingIndirectRequirement)
            {
                this.Availability = DataExtensionAvailability.MissingIndirectRequirement;
            }

            if (this.Availability == DataExtensionAvailability.MissingIndirectRequirement)
            {
                return;
            }

            if (availability == DataExtensionAvailability.Available)
            {
                this.Availability = availability;
            }
        }

        public void AddError(string error)
        {
            Guard.NotNullOrWhiteSpace(error, nameof(error));

            this.errors.Add(error);
        }

        private void ClearMessages()
        {
            this.errors.Clear();
            this.missingDataCookers.Clear();
            this.missingDataProcessors.Clear();
        }

        /// <summary>
        ///     This will process all of the data cooker requirements for the target data
        ///     extension (both source and composite). It will recursively descend through requirements,
        ///     determining if the target data extension may be used.
        /// </summary>
        /// <param name="availableDataExtensions">
        ///     Data extensions that have been exposed through the runtime.
        /// </param>
        private void ValidateDataCookerDependencies(
            IDataExtensionRepository availableDataExtensions)
        {
            Guard.NotNull(availableDataExtensions, nameof(availableDataExtensions));

            foreach (var requiredDataCookerPath in this.target.RequiredDataCookers)
            {
                IDataCookerReference dataCookerReference = null;

                var sourceId = requiredDataCookerPath.SourceParserId;
                if (string.IsNullOrWhiteSpace(sourceId))
                {
                    // Add composite data cooker

                    var requiredCookerReference = availableDataExtensions.GetCompositeDataCookerReference(requiredDataCookerPath);
                    if (requiredCookerReference == null)
                    {
                        this.UpdateAvailability(DataExtensionAvailability.MissingRequirement);
                        this.missingDataCookers.Add(requiredDataCookerPath);
                        continue;
                    }

                    dataCookerReference = requiredCookerReference;

                    this.dependencyReferences.AddRequiredCompositeDataCookerPath(requiredCookerReference.Path);
                }
                else
                {
                    // Add source data cooker

                    var requiredCookerReference = availableDataExtensions.GetSourceDataCookerReference(requiredDataCookerPath);
                    if (requiredCookerReference == null)
                    {
                        this.UpdateAvailability(DataExtensionAvailability.MissingRequirement);
                        this.missingDataCookers.Add(requiredDataCookerPath);
                        continue;
                    }

                    dataCookerReference = requiredCookerReference;

                    this.dependencyReferences.AddRequiredSourceDataCookerPath(requiredCookerReference.Path);
                }

                this.ProcessRequiredExtension(dataCookerReference, requiredDataCookerPath.CookerPath, availableDataExtensions);
            }
        }

        /// <summary>
        ///     This will process all of the data processor requirements for the target data
        ///     extension. It will recursively descend through requirements, determining if
        ///     the target data extension may be used.
        /// </summary>
        /// <param name="availableDataExtensions">
        ///     Data extensions that have been exposed through the runtime.
        /// </param>
        private void ValidateRequiredDataProcessors(
            IDataExtensionRepository availableDataExtensions)
        {
            Guard.NotNull(availableDataExtensions, nameof(availableDataExtensions));

            foreach (var processorId in this.target.RequiredDataProcessors)
            {
                var processorReference = availableDataExtensions.GetDataProcessorReference(processorId);
                if (processorReference == null)
                {
                    this.UpdateAvailability(DataExtensionAvailability.MissingRequirement);
                    this.missingDataProcessors.Add(processorId);
                    continue;
                }

                this.ProcessRequiredExtension(processorReference, processorId.Id, availableDataExtensions);
            }
        }

        private void ProcessRequiredExtension(
            IDataExtensionReference reference, 
            string extensionId,
            IDataExtensionRepository availableDataExtensions)
        {
            Guard.NotNull(reference, nameof(reference));
            Guard.NotNull(availableDataExtensions, nameof(availableDataExtensions));
            Debug.Assert(!string.IsNullOrWhiteSpace(extensionId));

            if (reference.Availability == DataExtensionAvailability.Undetermined)
            {
                reference.ProcessDependencies(availableDataExtensions);
            }

            if (reference.Availability != DataExtensionAvailability.Error)
            {
                this.dependencyReferences.AddRequiredExtensionReferences(reference.DependencyReferences);
            }

            Debug.Assert(
                reference.Availability != DataExtensionAvailability.Undetermined,
                $"{nameof(this.ProcessDependencies)} returned without establishing Availability.");

            this.UpdateAvailabilityFromRequiredExtension(reference, extensionId);

            this.target.PerformAdditionalDataExtensionValidation(this, reference);
        }

        /// <summary>
        ///     Update the availability for the target data extension based on one of its required
        ///     data extension's availability.
        /// </summary>
        /// <param name="requiredDataExtension">
        ///     The required extension.
        /// </param>
        /// <param name="extensionId">
        ///     Data extension Id.
        /// </param>
        private void UpdateAvailabilityFromRequiredExtension(
            IDataExtensionReference requiredDataExtension,
            string extensionId)
        {
            Debug.Assert(requiredDataExtension != null, nameof(requiredDataExtension));
            Debug.Assert(!string.IsNullOrWhiteSpace(extensionId));

            switch (requiredDataExtension.Availability)
            {
                case DataExtensionAvailability.Undetermined:
                    this.errors.Add("Internal error encountered.");
                    this.UpdateAvailability(DataExtensionAvailability.Error);
                    break;

                case DataExtensionAvailability.Error:
                    this.UpdateAvailability(DataExtensionAvailability.IndirectError);
                    this.errors.Add($"An error occurred in a required data cooker path: {extensionId}.");
                    break;

                case DataExtensionAvailability.MissingRequirement:
                    this.UpdateAvailability(DataExtensionAvailability.MissingIndirectRequirement);
                    break;

                case DataExtensionAvailability.MissingIndirectRequirement:
                    this.UpdateAvailability(this.Availability = DataExtensionAvailability.MissingIndirectRequirement);
                    break;
            }
        }
    }
}
