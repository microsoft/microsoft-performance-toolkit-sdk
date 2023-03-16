// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a repository for storing a collection of entities of type <typeparamref name="TEntity"/>.
    ///     Supports read-only operations and notifications when the repository is modified.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of the resources stored in this repository.
    /// </typeparam>
    public interface IReadonlyRepository<TEntity>
    {
        /// <summary>
        ///     Gets all plugins manager resources contained in this repository.
        /// </summary>
        IEnumerable<TEntity> Items { get; }

        /// <summary>
        ///    Raised when new resources are added to this repository.
        /// </summary>
        event EventHandler ItemsModified;
    }
}
