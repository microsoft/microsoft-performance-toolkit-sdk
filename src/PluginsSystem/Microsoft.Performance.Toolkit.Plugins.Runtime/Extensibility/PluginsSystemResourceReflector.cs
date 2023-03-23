// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     This class registers to with a provider to receive Types that might be
    ///     <see cref="Plugins.Core.Extensibility.IPluginsSystemResource"/>s.
    ///     The types are evaluated and stored as a <see cref="PluginsSystemResourceReference"/> when applicable.
    /// </summary>
    public sealed class PluginsSystemResourceReflector
        : IExtensionTypeObserver
    {
        private readonly Dictionary<Type, PluginsSystemResourceReference> loadedResources;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsSystemResourceReflector"/>
        ///     class, registering to receive updates from the given <see cref="IExtensionTypeProvider"/>
        /// </summary>
        /// <param name="extensionTypeProvider">
        ///     The object doing extension discovery.
        /// </param>
        public PluginsSystemResourceReflector(IExtensionTypeProvider extensionTypeProvider)
        {
            this.loadedResources = new Dictionary<Type, PluginsSystemResourceReference>();
            extensionTypeProvider.RegisterTypeConsumer(this);
        }

        /// <inheritdoc />
        public void ProcessType(Type type, string sourceName)
        {
            if (PluginsSystemResourceReference.TryCreateReference(type, out PluginsSystemResourceReference reference))
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
        ///     Gets all loaded <see cref="PluginsSystemResourceReference"/>s observed by this object.
        /// </summary>
        public IEnumerable<PluginsSystemResourceReference> AllResources
        {
            get
            {
                return this.loadedResources.Values;
            }
        }
    }
}
