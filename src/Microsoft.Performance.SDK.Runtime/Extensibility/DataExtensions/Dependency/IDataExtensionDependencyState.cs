// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    /// <summary>
    ///     Builds a set of dependencies for a target data extension object.
    ///     This may be any type of extension: data cooker, data processor, table... as
    ///     long as it implements IDataExtensionDependencyTarget.
    ///
    ///     The base class <see cref="DataExtensionReference"/> implements
    ///     <see cref="IDataExtensionDependencyTarget"/>.
    ///
    ///     Maintains all data associated with dependencies for the target, including
    ///     status, error messages, and missing dependencies - as well as the data extension
    ///     references that make up the dependencies.
    /// </summary>
    public interface IDataExtensionDependencyState
        : IDataExtensionDependency,
          IDataExtensionDependencyStateSupport,
          System.ICloneable
    {
        /// <summary>
        ///     Gets error messages reported while establishing dependencies.
        /// </summary>
        IReadOnlyCollection<ErrorInfo> Errors { get; }

        /// <summary>
        ///     Gets data cookers that are required but unavailable.
        /// </summary>
        IReadOnlyCollection<DataCookerPath> MissingDataCookers { get; }

        /// <summary>
        ///     Gets data processors that are required but unavailable.
        /// </summary>
        IReadOnlyCollection<DataProcessorId> MissingDataProcessors { get; }
    }
}