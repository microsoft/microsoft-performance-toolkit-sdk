// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    public delegate bool TryCreateCustomDataSourceReferenceDelegate(
        Type type,
        out CustomDataSourceReference reference);

    /// <summary>
    ///     This class registers to with a provider to receive Types that might be custom data sources.
    ///     The types are evaluated and stored as a <see cref="CustomDataSourceReference"/> when applicable.
    /// </summary>
    public sealed class ReflectionPlugInCatalog
        : IPlugInCatalog,
          IExtensionTypeObserver
    {
        private readonly TryCreateCustomDataSourceReferenceDelegate referenceFactory;

        private readonly Dictionary<Type, CustomDataSourceReference> loadedDataSources;

        public ReflectionPlugInCatalog(IExtensionTypeProvider extensionDiscovery)
            : this(
                  extensionDiscovery,
                  CustomDataSourceReference.TryCreateReference)
        {
        }

        public ReflectionPlugInCatalog(
            IExtensionTypeProvider extensionDiscovery,
            TryCreateCustomDataSourceReferenceDelegate referenceFactory)
        {
            Guard.NotNull(extensionDiscovery, nameof(extensionDiscovery));
            Guard.NotNull(referenceFactory, nameof(referenceFactory));

            this.referenceFactory = referenceFactory;

            this.loadedDataSources = new Dictionary<Type, CustomDataSourceReference>();
            this.IsLoaded = false;

            extensionDiscovery.RegisterTypeConsumer(this);
        }

        public IEnumerable<CustomDataSourceReference> PlugIns => this.loadedDataSources.Values;

        public bool IsLoaded { get; private set; }

        public void ProcessType(Type type, string sourceName)
        {
            if (this.referenceFactory(type, out CustomDataSourceReference reference))
            {
                Debug.Assert(reference != null);
                try
                {
                    this.loadedDataSources.Add(type, reference);
                }
                catch { }                
            }
        }

        public void DiscoveryComplete()
        {
            this.IsLoaded = true;
        }
    }
}
