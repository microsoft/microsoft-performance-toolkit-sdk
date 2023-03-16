using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a repository for storing a collection of entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the resources stored in this repository.
    /// </typeparam>
    public interface IReadonlyRepository<T>
    {
        /// <summary>
        ///     Gets all plugins manager resources contained in this repository.
        /// </summary>
        IEnumerable<T> Items { get; }

        /// <summary>
        ///    Raised when new resources are added to this repository.
        /// </summary>
        event EventHandler ItemsModified;
    }
}
