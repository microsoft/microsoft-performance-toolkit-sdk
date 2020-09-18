// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataProcessing;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     This class adds data cooker specific functionality on top of the base
    ///     <see cref="DataExtensionReference{TDerived}"/>.
    /// </summary>
    /// <typeparam name="TDerived">
    ///     The class deriving from this type.
    /// </typeparam>
    internal abstract class BaseDataCookerReference<TDerived>
        : DataExtensionReference<TDerived>
        where TDerived : BaseDataCookerReference<TDerived>
    {
        protected BaseDataCookerReference(Type type)
            : base(type)
        {
        }

        protected BaseDataCookerReference(DataExtensionReference<TDerived> other)
            : base(other)
        {
        }

        public DataCookerPath Path { get; private set; }

        public string Description { get; protected set; }

        public override string Name => this.Path.ToString();

        protected bool IsSourceDataCooker { get; set; }

        public override bool Equals(TDerived other)
        {
            return base.Equals(other) &&
                   StringComparer.Ordinal.Equals(this.Path, other.Path) &&
                   StringComparer.Ordinal.Equals(this.Description, other.Description);
        }

        protected virtual void ValidateInstance(IDataCookerDescriptor instance)
        {
            // Create an instance just to pull out the descriptor without saving any references to it.
            if (instance is null)
            {
                this.AddError($"Unable to create an instance of {this.Type}");
                this.InitialAvailability = DataExtensionAvailability.Error;
                return;
            }

            if (string.IsNullOrWhiteSpace(instance.Path.DataCookerId))
            {
                this.AddError("A data cooker Id may not be empty or null.");
                this.InitialAvailability = DataExtensionAvailability.Error;
            }
        }

        protected void InitializeDescriptorData(IDataCookerDescriptor descriptor)
        {
            Guard.NotNull(descriptor, nameof(descriptor));

            this.Path = descriptor.Path;
            this.Description = descriptor.Description;

            if (this.IsSourceDataCooker)
            {
                if (string.IsNullOrEmpty(descriptor.Path.SourceParserId))
                {
                    this.AddError($"A source data cooker's source Id must not be {nameof(DataCookerPath.EmptySourceParserId)}.");
                    this.InitialAvailability = DataExtensionAvailability.Error;
                }
            }
            else
            {
                if (descriptor.Path.SourceParserId != DataCookerPath.EmptySourceParserId)
                {
                    this.AddError($"A composite data cooker's source Id must be {nameof(DataCookerPath.EmptySourceParserId)}.");
                    this.InitialAvailability = DataExtensionAvailability.Error;
                }
            }

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
    }
}
