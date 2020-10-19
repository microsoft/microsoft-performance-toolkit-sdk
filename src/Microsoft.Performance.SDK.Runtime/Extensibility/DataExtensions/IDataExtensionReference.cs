// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     This interface combines interfaces that are required on all data extension references.
    /// </summary>
    public interface IDataExtensionReference
        : IDataExtensionDependencyTarget,
          IDataExtensionDependency
    {
    }
}
