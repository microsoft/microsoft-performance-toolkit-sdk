// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     This class registers to with a provider to receive Types that might be
    ///     <see cref="Plugins.Core.Extensibility.IPluginManagerResource"/>s.
    ///     The types are evaluated and stored as a <see cref="PluginManagerResourceReference"/> when applicable.
    /// </summary>
    public sealed class PluginManagerResourceReflector
        : IExtensionTypeObserver
    {
        private readonly Dictionary<Type, PluginManagerResourceReference> loadedResources;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginManagerResourceReflector"/>
        ///     class, registering to receive updates from the given <see cref="IExtensionTypeProvider"/>
        /// </summary>
        /// <param name="extensionTypeProvider">
        ///     The object doing extension discovery.
        /// </param>
        public PluginManagerResourceReflector(IExtensionTypeProvider extensionTypeProvider)
        {
            this.loadedResources = new Dictionary<Type, PluginManagerResourceReference>();
            extensionTypeProvider.RegisterTypeConsumer(this);
        }

        /// <inheritdoc />
        public void ProcessType(Type type, string sourceName)
        {
            if (PluginManagerResourceReference.TryCreateReference(type, out PluginManagerResourceReference reference))
            {
                try
                {
                    this.loadedResources.Add(type, reference);
                }
                catch { }
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
        ///     Gets all loaded <see cref="PluginManagerResourceReference"/>s observed by this object.
        /// </summary>
        public IEnumerable<PluginManagerResourceReference> AllResources
        {
            get
            {
                return this.loadedResources.Values;
            }
        }
    }
}
