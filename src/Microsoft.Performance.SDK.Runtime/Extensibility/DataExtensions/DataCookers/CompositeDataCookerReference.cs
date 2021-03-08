// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     Represents a reference to a composite data cooker.
    /// </summary>
    internal class CompositeDataCookerReference
        : BaseDataCookerReference<CompositeDataCookerReference>,
          ICompositeDataCookerReference
    {
        /// <summary>
        ///     This object is used to create synchronized regions
        ///     within the instance.
        /// </summary>
        protected readonly object instanceLock = new object();

        private bool initialized = false;
        private ICompositeDataCookerDescriptor instance = null;

        private bool isDisposed = false;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeDataCookerReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///     The instance from which to create this instance.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     <paramref name="other"/> is disposed.
        /// </exception>
        public CompositeDataCookerReference(CompositeDataCookerReference other)
            : base(other)
        {
            this.InitializeDescriptorData(other);

            lock (other.instanceLock)
            {
                this.Instance = other.Instance;
                this.InstanceInitialized = other.InstanceInitialized;
            }
        }

        private CompositeDataCookerReference(Type type)
            : base(type)
        {
            this.CreateInstance();

            this.ValidateInstance(this.Instance);

            if (this.Instance != null)
            {
                this.InitializeDescriptorData(this.Instance);
            }
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="CompositeDataCookerReference"/>
        ///     class.
        /// </summary>
        ~CompositeDataCookerReference()
        {
            this.Dispose(false);
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="ICompositeDataCookerReference"/> based on the
        ///     <paramref name="candidateType"/>.
        ///     <para/>
        ///     A <see cref="Type"/> must satisfy the following criteria in order to 
        ///     be eligible as a reference:
        ///     <list type="bullet">
        ///         <item>must be public.</item>
        ///         <item>must be concrete.</item>
        ///         <item>must implement ICompositeDataCookerDescriptor somewhere in the inheritance heirarchy (either directly or indirectly.)</item>
        ///         <item>must have a public parameterless constructor.</item>
        ///     </list>
        /// </summary>
        /// <param name="candidateType">
        ///     Candidate <see cref="Type"/> for the <see cref="ICompositeDataCookerReference"/>
        /// </param>
        /// <param name="reference">
        ///     Out <see cref="ICompositeDataCookerReference"/>
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of <see cref="ISourceDataCookerReference"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        internal static bool TryCreateReference(
            Type candidateType,
            out ICompositeDataCookerReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;

            if (candidateType.IsPublicAndInstantiatableOfType(typeof(ICompositeDataCookerDescriptor)))
            {
                // There must be an empty, public constructor
                if (candidateType.TryGetEmptyPublicConstructor(out var constructor))
                {
                    try
                    {
                        reference = new CompositeDataCookerReference(candidateType);
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.Error.WriteLine($"Cooker Disabled: {candidateType}.");
                        Console.Error.WriteLine($"{e.Message}");
                        return false;
                    }
                }
            }

            return reference != null;
        }

        /// <summary>
        ///     Gets or sets the instance of the cooker referneced
        ///     by this instance. Please use <see cref="InstanceInitialized"/>
        ///     to determine whether the instance reference by this property
        ///     has been initialized, even if the value is not <c>null</c>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected ICompositeDataCookerDescriptor Instance
        {
            get
            {
                this.ThrowIfDisposed();
                return this.instance;
            }
            set
            {
                this.ThrowIfDisposed();
                this.instance = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whethr the <see cref="Instance"/>
        ///     has been initialized.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        private bool InstanceInitialized
        {
            get
            {
                this.ThrowIfDisposed();
                return this.initialized;
            }
            set
            {
                this.ThrowIfDisposed();
                this.initialized = value;
            }
        }

        /// <inheritdoc />
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IDataCooker GetOrCreateInstance(IDataExtensionRetrieval requiredData)
        {
            Guard.NotNull(requiredData, nameof(requiredData));
            this.ThrowIfDisposed();

            if (this.Instance == null)
            {
                return null;
            }

            if (!this.InstanceInitialized)
            {
                lock (this.instanceLock)
                {
                    if (!this.InstanceInitialized)
                    {
                        this.Instance.OnDataAvailable(requiredData);
                        this.InstanceInitialized = true;
                    }
                }
            }

            return this.Instance;
        }

        /// <inheritdoc />
        public override CompositeDataCookerReference CloneT()
        {
            this.ThrowIfDisposed();
            return new CompositeDataCookerReference(this);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.instance.TryDispose();
                this.instance = null;
            }

            this.isDisposed = true;
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void ValidateInstance(IDataCookerDescriptor instance)
        {
            this.ThrowIfDisposed();
            base.ValidateInstance(instance);

            if (!StringComparer.Ordinal.Equals(instance.Path.SourceParserId, DataCookerPath.EmptySourceParserId))
            {
                this.AddError($"Unable to create an instance of {this.Type}");
                this.InitialAvailability = DataExtensionAvailability.Error;
            }
        }

        private void CreateInstance()
        {
            this.Instance = Activator.CreateInstance(this.Type) as ICompositeDataCookerDescriptor;
            Debug.Assert(this.Instance != null);
        }
    }
}
