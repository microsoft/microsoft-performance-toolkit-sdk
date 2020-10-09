// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    /// <summary>
    ///     Provides the dependency state of a data extension.
    /// </summary>
    public interface IDataExtensionDependency
    {
        /// <summary>
        ///     Gets whether all of the requirements for this data
        ///     extension are available.
        /// </summary>
        DataExtensionAvailability Availability { get; }

        /// <summary>
        ///     Gets the interface used to retrieve the dependency references of this extension reference.
        /// </summary>
        IDataExtensionDependencies DependencyReferences { get; }

        /// <summary>
        ///     Checks whether all of the requirements for this data extension
        ///     are available.
        /// </summary>
        /// <param name="availableDataExtensions">
        ///     Available data extensions.
        /// </param>
        void ProcessDependencies(IDataExtensionRepository availableDataExtensions);
    }
}
