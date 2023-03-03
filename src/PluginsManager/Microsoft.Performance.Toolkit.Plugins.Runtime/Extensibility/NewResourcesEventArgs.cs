// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Event args for <see cref="IPluginsManagerResourceRepository{T}.ResourcesAdded".
    /// </summary>
    /// <typeparam name="T">
    ///     Resource type.
    /// </typeparam>
    public class NewResourcesEventArgs<T>
        : EventArgs
        where T : IPluginsManagerResource
    {
        /// <summary>
        ///     Initializes a new <see cref="NewResourcesEventArgs"/>.
        /// </summary>
        /// <param name="newPluginsManagerResources"></param>
        public NewResourcesEventArgs(IEnumerable<T> newPluginsManagerResources)
        {
            this.NewPluginsManagerResources = newPluginsManagerResources;
        }

        /// <summary>
        ///     A collection of newly added resources.
        /// </summary>
        public IEnumerable<T> NewPluginsManagerResources { get; }
    }
}
