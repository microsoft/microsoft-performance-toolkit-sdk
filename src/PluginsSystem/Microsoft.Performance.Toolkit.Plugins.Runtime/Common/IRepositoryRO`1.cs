// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.Specialized;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a repository for storing a collection of entities of type <typeparamref name="TEntity"/>.
    ///     Supports read-only operations and notifications when the repository is modified.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepositoryRO<TEntity>
        : INotifyCollectionChanged
    {
        /// <summary>
        ///     Gets all resources contained in this repository.
        /// </summary>
        IEnumerable<TEntity> Items { get; }
    }
}
