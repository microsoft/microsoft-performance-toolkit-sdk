// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <inheritdoc />
    public sealed class PluginManagerResourceLoader
        : IPluginManagerResourceLoader
    {
        private readonly object mutex = new object();
        private readonly AssemblyExtensionDiscovery extensionDiscovery;

        private readonly HashSet<IPluginManagerResourcesConsumer> subscribers;
        private readonly PluginManagerResourceReflector resourcesReflector;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginManagerResourceLoader"/>
        /// </summary>
        /// <param name="assemblyLoader">
        ///     Loads assemblies.
        /// </param>
        /// <param name="validatorFactory">
        ///     Creates <see cref="IPreloadValidator"/> instances to make
        ///     sure candidate assemblies are valid to even try to load.
        ///     The function takes a collection of file names and returns
        ///     a new <see cref="IPreloadValidator"/> instance. This function
        ///     should never return <c>null</c>.
        /// </param>
        public PluginManagerResourceLoader(
            IAssemblyLoader assemblyLoader,
            Func<IEnumerable<string>, IPreloadValidator> validatorFactory)
        {
            this.subscribers = new HashSet<IPluginManagerResourcesConsumer>();
            this.extensionDiscovery = new AssemblyExtensionDiscovery(assemblyLoader, validatorFactory);
            this.resourcesReflector = new PluginManagerResourceReflector(this.extensionDiscovery);
        }
        
        /// <inheritdoc />
        public bool TryLoad(string directory)
        {
            Guard.NotNull(directory, nameof(directory));

            lock (this.mutex)
            {
                var oldResources = new HashSet<PluginManagerResourceReference>(this.resourcesReflector.AllResources);

                bool success = this.extensionDiscovery.ProcessAssemblies(directory, out ErrorInfo error);

                IEnumerable<PluginManagerResourceReference> newResources = this.resourcesReflector.AllResources.Except(oldResources);
                NotifyResourceLoaded(newResources);
                
                return success;
            }
        }

        /// <inheritdoc />
        public bool Subscribe(IPluginManagerResourcesConsumer consumer)
        {
            Guard.NotNull(consumer, nameof(consumer));

            lock (this.mutex)
            {
                if (this.subscribers.Contains(consumer))
                {
                    return false;
                }

                // Notify consumer of exisiting resources.
                consumer.OnNewResourcesLoaded(this.resourcesReflector.AllResources);
                
                this.subscribers.Add(consumer);

                return true;
            }
        }

        /// <inheritdoc />
        public bool Unsubscribe(IPluginManagerResourcesConsumer consumer)
        {
            Guard.NotNull(consumer, nameof(consumer));

            lock (this.mutex)
            {
                return this.subscribers.Remove(consumer);
            }
        }
        
        private void NotifyResourceLoaded(IEnumerable<PluginManagerResourceReference> resources)
        {
            foreach (IPluginManagerResourcesConsumer subscriber in this.subscribers)
            {
                subscriber.OnNewResourcesLoaded(resources);
            }
        }
    }
}
