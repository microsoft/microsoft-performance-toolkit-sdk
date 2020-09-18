// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataProcessing;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    /// <summary>
    ///     This interface is meant to for data extension references to interact with
    ///     <see cref="DataExtensionDependencyState"/>, enabling it to establish the availability
    ///     of the data extension based on the status of its data extension requirements.
    /// </summary>
    public interface IDataExtensionDependencyTarget
        : IDataCookerDependent,
          IDataProcessorDependent
    {
        /// <summary>
        ///     Gets an identifier for the given data extension.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the initial availability of the target.
        ///     If something went wrong early, this initial availability will indicate
        ///     such. If it's set to error, then no further validation will be performed.
        /// </summary>
        DataExtensionAvailability InitialAvailability { get; }

        /// <summary>
        ///     An extensibility point to allow for data extension specific
        ///     validation.
        /// </summary>
        /// <param name="dependencyStateSupport">
        ///     Provides services for validation.
        /// </param>
        /// <param name="requiredDataExtension">
        ///     The required data extension to be validated.
        /// </param>
        void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataExtension);
    }
}
