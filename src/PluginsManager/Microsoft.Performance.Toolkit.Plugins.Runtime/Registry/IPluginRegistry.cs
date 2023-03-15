// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a registry that stores information about installed plugins and supports registering, unregistering and updating plugins.
    ///     This registry also provides a lock that can be used to synchronize access to the registry.
    /// </summary>
    /// <typeparam name="TRoot">
    ///    The type of the root path of the registry.
    /// </typeparam>
    public interface IPluginRegistry<TRoot>
        : IRepository<InstalledPluginInfo, string>,
          ISynchronizedObject
    {
        /// <summary>
        ///     Gets the root path of the registry.
        /// </summary>
        TRoot RegistryRoot { get; }
    }
}
