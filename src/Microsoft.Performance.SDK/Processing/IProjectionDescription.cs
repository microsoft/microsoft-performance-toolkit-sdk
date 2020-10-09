// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// Exposes information about a function that has one parameter of a specified type and returns a value of another specified type.
    /// </summary>
    public interface IProjectionDescription
    {
        /// <summary>
        /// Gets the type of the parameter of the function representing the selector.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// Gets the type of the values returned by the function representing the selector.
        /// </summary>
        Type ResultType { get; }
    }
}
