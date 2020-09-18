// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    /// <summary>
    ///     This is passed to a data extension in calls to
    ///     <see cref="IDataExtensionDependencyTarget.PerformAdditionalDataExtensionValidation"/>
    ///     so that it may update dependency state.
    /// </summary>
    public interface IDataExtensionDependencyStateSupport
    {
        /// <summary>
        ///     Add an error message while processing dependencies.
        /// </summary>
        /// <param name="error">
        ///     Error message.
        /// </param>
        void AddError(string error);

        /// <summary>
        ///     Sets the data extension availability.
        /// </summary>
        /// <param name="availability">
        ///     The availability.
        /// </param>
        void UpdateAvailability(DataExtensionAvailability availability);
    }
}
