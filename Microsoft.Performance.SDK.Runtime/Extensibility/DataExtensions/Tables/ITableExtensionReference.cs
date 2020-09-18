// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables
{
    /// <summary>
    ///     A wrapper around a type that is a table data extension.
    /// </summary>
    public interface ITableExtensionReference
        : ITableExtension,
          IDataExtensionDependency
    {
    }
}
