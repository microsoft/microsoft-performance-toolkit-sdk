// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     A function that takes a <see cref="Type"/> and attempts to create
    ///     a <see cref="ProcessingSourceReference"/> from the given <see cref="Type"/>.
    ///     See <see cref="ProcessingSourceReference.TryCreateReference(Type, out ProcessingSourceReference)"/>
    ///     for details as to what requirements a <see cref="Type"/> must
    ///     fulfill in order to be eligible for creating a reference.
    /// </summary>
    /// <param name="type">
    ///     The <see cref="Type"/> from which to try to create the reference.
    /// </param>
    /// <param name="reference">
    ///     If a <see cref="ProcessingSourceReference"/> is able to be created,
    ///     this parameter will receive the created reference. If a reference
    ///     is not able to be created, this parameter will be set to <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if a reference is created from the given <see cref="Type"/>;
    ///     <c>false</c> otherwise. If <c>true</c> is returned, <paramref name="reference"/>
    ///     will be set to a non-null value. If <c>false</c> is returned, <paramref name="reference" />
    ///     will be set to <c>null</c>.
    /// </returns>
    public delegate bool TryCreateProcessingSourceReferenceDelegate(
        Type type,
        out ProcessingSourceReference reference);

    /// <summary>
    ///     This class registers to with a provider to receive Types that might be custom data sources.
    ///     The types are evaluated and stored as a <see cref="ProcessingSourceReference"/> when applicable.
    /// </summary>
    public sealed class ReflectionPlugInCatalog
        : IPlugInCatalog,
          IExtensionTypeObserver
    {
        private TryCreateProcessingSourceReferenceDelegate referenceFactory;

        private Dictionary<Type, ProcessingSourceReference> loadedDataSources;

        private bool isLoaded;

        private bool isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReflectionPlugInCatalog"/>
        ///     class, registering to receive extension updates with the given
        ///     discovery interface.
        /// </summary>
        /// <param name="extensionDiscovery">
        ///     The object doing extension discovery.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="extensionDiscovery"/> is <c>null</c>.
        /// </exception>
        public ReflectionPlugInCatalog(IExtensionTypeProvider extensionDiscovery)
            : this(
                  extensionDiscovery,
                  ProcessingSourceReference.TryCreateReference)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReflectionPlugInCatalog"/>
        ///     class, registering to receive extension updates with the given
        ///     discovery interface.
        /// </summary>
        /// <param name="extensionDiscovery">
        ///     The object doing extension discovery.
        /// </param>
        /// <param name="referenceFactory">
        ///     The function to use to try to create references from a given
        ///     <see cref="Type"/>. See <see cref="TryCreateProcessingSourceReferenceDelegate"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="extensionDiscovery"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="referenceFactory"/> is <c>null</c>.
        /// </exception>
        public ReflectionPlugInCatalog(
            IExtensionTypeProvider extensionDiscovery,
            TryCreateProcessingSourceReferenceDelegate referenceFactory)
        {
            Guard.NotNull(extensionDiscovery, nameof(extensionDiscovery));
            Guard.NotNull(referenceFactory, nameof(referenceFactory));

            this.referenceFactory = referenceFactory;
            this.isDisposed = false;

            this.loadedDataSources = new Dictionary<Type, ProcessingSourceReference>();
            this.isLoaded = false;

            extensionDiscovery.RegisterTypeConsumer(this);
        }

        /// <inheritdoc />
        public IEnumerable<ProcessingSourceReference> PlugIns
        {
            get
            {
                this.ThrowIfDisposed();
                return this.loadedDataSources.Values;
            }
        }

        /// <inheritdoc />

        public bool IsLoaded
        {
            get
            {
                this.ThrowIfDisposed();
                return this.isLoaded;
            }
            private set
            {
                this.ThrowIfDisposed();
                this.isLoaded = value;
            }
        }

        /// <inheritdoc />
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public void ProcessType(Type type, string sourceName)
        {
            this.ThrowIfDisposed();
            if (this.referenceFactory(type, out ProcessingSourceReference reference))
            {
                Debug.Assert(reference != null);
                try
                {
                    this.loadedDataSources.Add(type, reference);
                }
                catch { }                
            }
        }

        /// <inheritdoc />
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public void DiscoveryStarted()
        {
            this.IsLoaded = false;
        }

        /// <inheritdoc />
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public void DiscoveryComplete()
        {
            this.IsLoaded = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disoses all resources held by this class.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to dispose both managed and unmanaged
        ///     resources; <c>false</c> to dispose only unmanaged
        ///     resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var cds in this.PlugIns)
                {
                    cds.SafeDispose();
                }

                this.loadedDataSources = null;
                this.referenceFactory = null;
            }

            this.isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(ReflectionPlugInCatalog));
            }
        }
    }
}
