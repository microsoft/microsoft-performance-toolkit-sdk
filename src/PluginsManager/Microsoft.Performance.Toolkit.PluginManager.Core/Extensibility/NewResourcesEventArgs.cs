// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Extensibility
{
    /// <summary>
    ///     Event args for <see cref="ResourceRepository{T}.ResourcesAdded".
    /// </summary>
    /// <typeparam name="T">
    ///     Resource type.
    /// </typeparam>
    public class NewResourcesEventArgs<T> : EventArgs where T : IPluginManagerResource
    {
        /// <summary>
        ///     Initializes a new <see cref="NewResourcesEventArgs"/>.
        /// </summary>
        /// <param name="newPluginResources"></param>
        public NewResourcesEventArgs(IEnumerable<T> newPluginResources)
        {
            this.NewPluginResources = newPluginResources;
        }

        /// <summary>
        ///     A collection of newly added resources.
        /// </summary>
        public IEnumerable<T> NewPluginResources { get; }
    }
}
