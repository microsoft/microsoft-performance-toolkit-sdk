// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     This interface combines interfaces that are required on all data extension references.
    /// </summary>
    public interface IDataExtensionReference
        : IDataExtensionDependencyTarget,
          IDataExtensionDependency,
          IDisposable
    {
        /// <summary>
        ///     Gets the state of the dependencies of this
        ///     instance. This property is not valid until
        ///     <see cref="IDataExtensionDependency.ProcessDependencies(Repository.IDataExtensionRepository)"/>
        ///     has been called.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        IDataExtensionDependencyState DependencyState { get; }
    }
}
