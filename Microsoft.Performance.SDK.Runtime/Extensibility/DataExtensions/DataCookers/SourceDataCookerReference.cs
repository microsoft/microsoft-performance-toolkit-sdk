// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     This class provides a way to create an instance of a particular data extension.
    /// </summary>
    internal class SourceDataCookerReference
        : BaseDataCookerReference<SourceDataCookerReference>,
          ISourceDataCookerReference
    {
        public DataProductionStrategy ProductionStrategy { get; }

        internal static bool TryCreateReference(
            Type candidateType,
            out ISourceDataCookerReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;

            // perform this basic check first, as it's cheaper than a more specific test below
            if (!candidateType.Implements(typeof(IDataCooker)))
            {
                return false;
            }

            if (!candidateType.IsInstantiatable())
            {
                // this is ok, could just be an abstract base class for a data cooker
                return false;
            }

            if (!candidateType.GetInterfaces().Any(
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISourceDataCooker<,,>)))
            {
                // this is ok, it might be some other type of data cooker
                return false;
            }

            if (!candidateType.IsPublic())
            {
                Console.Error.WriteLine(
                    $"Warning: type {candidateType} seems to be a source data cooker, but is not public.");
                return false;
            }

            // There must be an empty, public constructor
            if (!candidateType.TryGetEmptyPublicConstructor(out var constructor))
            {
                Console.Error.WriteLine(
                    $"Warning: type {candidateType} seems to be a data cooker, but is missing an empty public " +
                    "constructor.");
                return false;
            }

            try
            {
                reference = new SourceDataCookerReference(candidateType);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Cooker Disabled: {candidateType}.");
                Console.Error.WriteLine($"{e.Message}");
                return false;
            }

            return true;
        }

        private SourceDataCookerReference(Type type)
            : base(type)
        {
            Guard.NotNull(type, nameof(type));

            // Create an instance just to pull out the descriptor without saving any references to it.

            var instance = this.CreateInstance();

            if (!(instance is ISourceDataCookerDescriptor sourceCookerDescriptor))
            {
                throw new ArgumentException("The type is not a recognized source data cooker.", nameof(type));
            }

            this.ProductionStrategy = sourceCookerDescriptor.DataProductionStrategy;

            if (sourceCookerDescriptor.DataProductionStrategy == DataProductionStrategy.AsRequired)
            {
                // _CDS_
                // I don't think this should be an issue, because PerfCore event sinks will use
                // EventSinkType.Context rather than going through this, but checking just in case.
                //

                if(StringComparer.Ordinal.Equals(this.Path.SourceParserId, "Microsoft.XPerf"))
                {
                    if (RequiredDataCookers.Any())
                    {
                        // _CDS_ todo: consider loosening this restriction to allow AsRequired source cookers to require other AsRequired source cookers
                        // if we do this, we'll need to decide how to handle this in PerfCore, which currently doesn't allow Context event sinks to have
                        // any dependencies. See "IsCompatibleDependency" in Session.cpp.
                        //

                        // An AsRequired SourceDataCooker must be able to run in every stage, and before source cookers
                        // that require it. XPerf doesn't allow a context data cooker to have any dependencies
                        throw new InvalidOperationException(
                            $"A SourceCooker whose {nameof(DataProductionStrategy)} is " +
                            $"{nameof(DataProductionStrategy.AsRequired)} can only consume SourceCookers whose " +
                            $"{nameof(DataProductionStrategy)} is also {nameof(DataProductionStrategy.AsRequired)}.");
                    }
                }
            }

            this.ValidateInstance(instance);

            this.IsSourceDataCooker = true;

            if (instance != null)
            {
                this.InitializeDescriptorData(instance);
            }
        }

        public SourceDataCookerReference(SourceDataCookerReference other)
            : base(other)
        {
            this.InitializeDescriptorData(other);
        }

        public override SourceDataCookerReference CloneT()
        {
            return new SourceDataCookerReference(this);
        }

        public IDataCookerDescriptor CreateInstance()
        {
            var value = Activator.CreateInstance(this.Type) as IDataCookerDescriptor;
            Debug.Assert(value != null);

            return value;
        }

        public override void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataExtension)
        {
            Guard.NotNull(dependencyStateSupport, nameof(dependencyStateSupport));
            Guard.NotNull(requiredDataExtension, nameof(requiredDataExtension));
            Debug.Assert(!string.IsNullOrWhiteSpace(this.Path.SourceParserId));

            // a source data cooker may not rely on any other source
            // a source data cooker may not rely on a non-source data cooker
            // a source data cooker may not rely on data processors

            if (requiredDataExtension is IDataCookerReference dataCookerReference)
            {
                if (!StringComparer.Ordinal.Equals(this.Path.SourceParserId, dataCookerReference.Path.SourceParserId))
                {
                    dependencyStateSupport.AddError("A source data cooker may not depend on data cookers from " +
                                                    $"other sources: {this.Type}");
                    dependencyStateSupport.UpdateAvailability(DataExtensionAvailability.Error);
                }
                else if (!(requiredDataExtension is SourceDataCookerReference))
                {
                    dependencyStateSupport.AddError(
                        $"Data cooker {dataCookerReference.Path} referenced by {this.Path} is unrecognized.");
                }

                if(ProductionStrategy == DataProductionStrategy.AsRequired)
                {
                    if (((SourceDataCookerReference)requiredDataExtension).ProductionStrategy != DataProductionStrategy.AsRequired)
                    {
                        throw new InvalidOperationException(
                            $"A SourceCooker whose {nameof(DataProductionStrategy)} is " +
                            $"{nameof(DataProductionStrategy.AsRequired)} can only consume SourceCookers whose " +
                            $"{nameof(DataProductionStrategy)} is also {nameof(DataProductionStrategy.AsRequired)}.");
                    }
                }
            }
            else if (requiredDataExtension is IDataProcessorReference dataProcessorReference)
            {
                dependencyStateSupport.AddError(
                    $"A source data cooker may not depend on a data processor: {dataProcessorReference.Id}");
                dependencyStateSupport.UpdateAvailability(DataExtensionAvailability.Error);
            }
            else
            {
                dependencyStateSupport.AddError(
                    $"A requested dependency on an unknown data extension type is not supported: {requiredDataExtension.Name}");
                dependencyStateSupport.UpdateAvailability(DataExtensionAvailability.Error);
            }
        }
    }
}
