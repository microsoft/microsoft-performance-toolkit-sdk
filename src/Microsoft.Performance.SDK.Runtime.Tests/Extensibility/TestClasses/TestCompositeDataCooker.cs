// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestCompositeDataCooker
        : ICompositeDataCookerDescriptor
    {
        public static DataCookerPath CookerPath = DataCookerPath.ForComposite(nameof(TestCompositeDataCooker));

        public TestCompositeDataCooker()
        {
            this.Path = TestCompositeDataCooker.CookerPath;
            this.RequiredDataCookers = new List<DataCookerPath>();
        }

        public string Description { get; set; }

        public DataCookerPath Path { get; set; }

        public IReadOnlyCollection<DataOutputPath> OutputIdentifiers { get; set; }

        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers { get; set; }

        public Action<IDataExtensionRetrieval> onDataAvailable { get; set; }
        public IDataExtensionRetrieval RequiredData { get; private set; }
        public void OnDataAvailable(IDataExtensionRetrieval requiredData)
        {
            RequiredData = requiredData;
            this.onDataAvailable?.Invoke(requiredData);
        }

        public T QueryOutput<T>(DataOutputPath identifier)
        {
            throw new System.NotImplementedException();
        }

        public object QueryOutput(DataOutputPath identifier)
        {
            throw new System.NotImplementedException();
        }

        public bool TryQueryOutput<T>(DataOutputPath identifier, out T result)
        {
            result = default;
            return false;
        }

        public bool TryQueryOutput(DataOutputPath identifier, out object result)
        {
            result = default;
            return false;
        }
    }
}
