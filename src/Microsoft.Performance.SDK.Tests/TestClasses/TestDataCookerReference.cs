// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public abstract class TestDataCookerReference
        : TestDataExtensionReference,
          IDataCookerReference
    {
        protected TestDataCookerReference()
            : this(true)
        {
        }

        protected TestDataCookerReference(bool useDataExtensionDependencyState)
            : base(useDataExtensionDependencyState)
        {
            Path = DataCookerPath.ForComposite(
                string.Concat(
                    GetType().Name,
                    " ",
                    Guid.NewGuid().ToString()));
        }

        protected TestDataCookerReference(Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
            : this(true, createDependencyState)
        {
        }

        protected TestDataCookerReference(
            bool useDataExtensionDependencyState,
            Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
            : base(useDataExtensionDependencyState, createDependencyState)
        {
            Path = DataCookerPath.ForComposite(
                string.Concat(
                    GetType().Name,
                    " ",
                    Guid.NewGuid().ToString()));
        }

        public string Description { get; set; }

        public DataCookerPath Path { get; set; }

        public override string Name => Path.CookerPath;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class TestSourceDataCookerReference
        : TestDataCookerReference,
          ISourceDataCookerReference
    {
        public TestSourceDataCookerReference()
            : base()
        {
        }

        public TestSourceDataCookerReference(bool useDataExtensionDependencyState)
            : base(useDataExtensionDependencyState)
        {
        }

        public TestSourceDataCookerReference(
            Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
            : this(true, createDependencyState)
        {
        }

        public TestSourceDataCookerReference(
            bool useDataExtensionDependencyState,
            Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
            : base(useDataExtensionDependencyState, createDependencyState)
        {
        }

        public Func<IDataCookerDescriptor> createInstance;
        public IDataCookerDescriptor CreateInstance()
        {
            return createInstance?.Invoke();
        }

        public bool UseDefaultValidation { get; set; } = true;

        public override void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataExtension)
        {
            if (UseDefaultValidation)
            {
                DefaultAdditionalValidation(dependencyStateSupport, requiredDataExtension);
            }
            else
            {
                base.PerformAdditionalDataExtensionValidation(
                    dependencyStateSupport,
                    requiredDataExtension);
            }
        }

        // this matches what we have in SourceDataCookerReference - it's not 100% necessary,
        // but it's a nice to have for testing some scenarios
        //
        public void DefaultAdditionalValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference reference)
        {
            if (reference is IDataCookerReference dataCookerReference)
            {
                if (!StringComparer.Ordinal.Equals(dataCookerReference.Path.SourceParserId, Path.SourceParserId))
                {
                    dependencyStateSupport.AddError(
                        new ErrorInfo(
                            ErrorCodes.EXTENSION_Error,
                            "Wrong source parser!"));
                    dependencyStateSupport.UpdateAvailability(DataExtensionAvailability.Error);
                }
            }
            // TODO: __SDK_DP__
            // Redesign Data Processor API
            ////else if (reference is IDataProcessorReference dataProcessorReference)
            ////{
            ////    dependencyStateSupport.AddError(
            ////        new ErrorInfo(
            ////            ErrorCodes.EXTENSION_DisallowedDataProcessorDependency,
            ////            $"A source data cooker may not depend on a data processor: {dataProcessorReference.Id}"));
            ////    dependencyStateSupport.UpdateAvailability(DataExtensionAvailability.Error);
            ////}
            else
            {
                dependencyStateSupport.AddError(
                    new ErrorInfo(
                        ErrorCodes.EXTENSION_UnknownDependencyType,
                        $"A requested dependency on an unknown data extension type is not supported: {reference.Name}"));
                dependencyStateSupport.UpdateAvailability(DataExtensionAvailability.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class TestCompositeDataCookerReference
        : TestDataCookerReference,
          ICompositeDataCookerReference
    {
        public TestCompositeDataCookerReference()
            : this(true)
        {
        }

        public TestCompositeDataCookerReference(bool useDataExtensionDependencyState)
            : base(useDataExtensionDependencyState)
        {
        }

        public TestCompositeDataCookerReference(
            Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
            : this(true, createDependencyState)
        {
        }

        public TestCompositeDataCookerReference(
            bool useDataExtensionDependencyState,
            Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
            : base(useDataExtensionDependencyState, createDependencyState)
        {
        }

        public Func<IDataCooker> createInstance;
        public IDataCooker CreateInstance(IDataExtensionRetrieval requiredData)
        {
            return createInstance?.Invoke();
        }

        public bool DisposeWasCalled { get; private set; }

        protected override void Dispose(bool disposing)
        {
            DisposeWasCalled = true;
            base.Dispose(disposing);
        }
    }
}
