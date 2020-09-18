// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using System;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    internal class CompositeDataCookerReference
        : BaseDataCookerReference<CompositeDataCookerReference>,
          ICompositeDataCookerReference
    {
        protected readonly object instanceLock = new object();

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

        protected ICompositeDataCookerDescriptor Instance { get; set; }

        private bool InstanceInitialized { get; set; }

        public IDataCooker GetOrCreateInstance(IDataExtensionRetrieval requiredData)
        {
            Guard.NotNull(requiredData, nameof(requiredData));

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

        public override CompositeDataCookerReference CloneT()
        {
            return new CompositeDataCookerReference(this);
        }

        protected override void ValidateInstance(IDataCookerDescriptor instance)
        {
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
