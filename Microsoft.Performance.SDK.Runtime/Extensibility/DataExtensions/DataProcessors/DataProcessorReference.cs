// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataProcessing;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors
{
    /// <summary>
    ///     A data processor is different than data cookers in that
    ///     their output is not static but may change based on input.
    ///     References to DataProcessor types don't create a singleton
    ///     instance of their target type. Instead, <see cref="GetOrCreateInstance"/>
    ///     should be called to create an instance based on input data.
    /// </summary>
    internal class DataProcessorReference
        : DataExtensionReference<DataProcessorReference>,
          IDataProcessorReference
    {
        private readonly object instanceLock = new object();

        internal static bool TryCreateReference(
            Type candidateType,
            out IDataProcessorReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;

            if (candidateType.IsPublicAndInstantiatableOfType(typeof(IDataProcessor)))
            {
                // There must be an empty, public constructor
                if (candidateType.TryGetEmptyPublicConstructor(out var constructor))
                {
                    try
                    {
                        reference = new DataProcessorReference(candidateType);
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.Error.WriteLine($"Data processor disabled: {candidateType}.");
                        Console.Error.WriteLine($"{e.Message}");
                        return false;
                    }
                }
            }

            return reference != null;
        }

        private DataProcessorReference(Type type)
            : base(type)
        {
            // Create an instance just to pull out the descriptor without saving any references to it.

            Debug.Assert(type != null, nameof(type));

            this.CreateInstance();
            if (this.Instance == null)
            {
                this.AddError($"Unable to create an instance of {this.Type}");
                this.InitialAvailability = DataExtensionAvailability.Error;
            }
            else if (string.IsNullOrWhiteSpace(this.Instance.Id))
            {
                this.AddError("A data processor Id may not be empty.");
                this.InitialAvailability = DataExtensionAvailability.Error;
            }

            if (this.InitialAvailability != DataExtensionAvailability.Error)
            {
                this.InitializeDescriptorData(this.Instance);
            }
        }

        public DataProcessorReference(DataProcessorReference other) 
            : base(other)
        {
            this.InitializeDescriptorData(other);

            lock (other.instanceLock)
            {
                this.Instance = other.Instance;
                this.InstanceInitialized = other.InstanceInitialized;
            }
        }

        public string Id { get; private set; }

        public string Description { get; private set; }

        private IDataProcessor Instance { get; set; }

        private bool InstanceInitialized { get; set; }

        public override DataProcessorReference CloneT()
        {
            return new DataProcessorReference(this);
        }

        public IDataProcessor GetOrCreateInstance(IDataExtensionRetrieval requiredData)
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

        private void InitializeDescriptorData(IDataProcessorDescriptor descriptor)
        {
            Guard.NotNull(descriptor, nameof(descriptor));

            this.Id = descriptor.Id;
            this.Description = descriptor.Description;

            if (descriptor is IDataCookerDependent cookerRequirements)
            {
                foreach (var dataCookerPath in cookerRequirements.RequiredDataCookers)
                {
                    this.AddRequiredDataCooker(dataCookerPath);
                }
            }

            if (descriptor is IDataProcessorDependent processorRequirements)
            {
                foreach (var dataProcessorId in processorRequirements.RequiredDataProcessors)
                {
                    this.AddRequiredDataProcessor(dataProcessorId);
                }
            }
        }

        public override bool Equals(DataProcessorReference other)
        {
            return base.Equals(other) &&
                   StringComparer.Ordinal.Equals(this.Id, other.Id) &&
                   StringComparer.Ordinal.Equals(this.Description, other.Description);
        }

        private void CreateInstance()
        {
            this.Instance = Activator.CreateInstance(this.Type) as IDataProcessor;
            Debug.Assert(this.Instance != null);
        }
    }
}
