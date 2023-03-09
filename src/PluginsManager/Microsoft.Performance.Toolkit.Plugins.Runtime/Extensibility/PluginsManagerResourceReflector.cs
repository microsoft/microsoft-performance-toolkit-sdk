// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     This class registers to with a provider to receive Types that might be
    ///     <see cref="Plugins.Core.Extensibility.IPluginsManagerResource"/>s.
    ///     The types are evaluated and stored as a <see cref="PluginsManagerResourceReference"/> when applicable.
    /// </summary>
    public sealed class PluginsManagerResourceReflector
        : IExtensionTypeObserver
    {
        private readonly Dictionary<Type, PluginsManagerResourceReference> loadedResources;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsManagerResourceReflector"/>
        ///     class, registering to receive updates from the given <see cref="IExtensionTypeProvider"/>
        /// </summary>
        /// <param name="extensionTypeProvider">
        ///     The object doing extension discovery.
        /// </param>
        public PluginsManagerResourceReflector(IExtensionTypeProvider extensionTypeProvider)
        {
            this.loadedResources = new Dictionary<Type, PluginsManagerResourceReference>();
            extensionTypeProvider.RegisterTypeConsumer(this);
        }

        /// <inheritdoc />
        public void ProcessType(Type type, string sourceName)
        {
            if (PluginsManagerResourceReference.TryCreateReference(type, out PluginsManagerResourceReference reference))
            {
                try
                {
                    this.loadedResources.Add(type, reference);
                }
                catch
                {
                    // TODO: #271 Add error handling and logging when re-enable loading additional resources.
                }
            }
        }

        /// <inheritdoc />
        public void DiscoveryStarted()
        {
        }

        /// <inheritdoc />
        public void DiscoveryComplete()
        {
        }

        /// <summary>
        ///     Gets all loaded <see cref="PluginsManagerResourceReference"/>s observed by this object.
        /// </summary>
        public IEnumerable<PluginsManagerResourceReference> AllResources
        {
            get
            {
                return this.loadedResources.Values;
            }
        }
    }
}
