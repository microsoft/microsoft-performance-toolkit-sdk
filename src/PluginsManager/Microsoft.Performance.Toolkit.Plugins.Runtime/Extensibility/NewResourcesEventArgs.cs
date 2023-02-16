// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Event args for <see cref="IPluginManagerResourceRepository{T}.ResourcesAdded".
    /// </summary>
    /// <typeparam name="T">
    ///     Resource type.
    /// </typeparam>
    public class NewResourcesEventArgs<T>
        : EventArgs
        where T : IPluginManagerResource
    {
        /// <summary>
        ///     Initializes a new <see cref="NewResourcesEventArgs"/>.
        /// </summary>
        /// <param name="newPluginManagerResources"></param>
        public NewResourcesEventArgs(IEnumerable<T> newPluginManagerResources)
        {
            this.NewPluginManagerResources = newPluginManagerResources;
        }

        /// <summary>
        ///     A collection of newly added resources.
        /// </summary>
        public IEnumerable<T> NewPluginManagerResources { get; }
    }
}
