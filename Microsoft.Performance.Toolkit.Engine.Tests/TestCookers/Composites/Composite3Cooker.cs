// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Composites
{
    public sealed class Composite3Cooker
        : CookedDataReflector,
          ICompositeDataCookerDescriptor
    {
        public static readonly DataCookerPath DataCookerPath = new DataCookerPath(nameof(Composite3Cooker));

        public Composite3Cooker()
            : base(DataCookerPath)
        {
            this.Output = new List<Composite3Output>();
        }

        public string Description => "Composite 3 cooker";

        public DataCookerPath Path => DataCookerPath;

        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => new[]
        {
            Composite1Cooker.DataCookerPath,
            Source5DataCooker.DataCookerPath,
        };

        [DataOutput]
        public List<Composite3Output> Output { get; }

        public void OnDataAvailable(IDataExtensionRetrieval requiredData)
        {
            var source5Data = requiredData.QueryOutput<List<Source5DataObject>>(
                new DataOutputPath(
                    Source5DataCooker.DataCookerPath,
                    nameof(Source5DataCooker.Objects)));
            var composite1Data = requiredData.QueryOutput<List<Composite1Output>>(
                new DataOutputPath(
                    Composite1Cooker.DataCookerPath,
                    nameof(Composite1Cooker.Output)));

            this.Output.Add(
                new Composite3Output
                {
                    Key = source5Data.Count + composite1Data.Count,
                    Composite1Count = composite1Data.Count,
                    Source5Count = source5Data.Count,
                });
        }
    }

    public struct Composite3Output
        : IKeyedDataObject<int>
    {
        public int Key { get; set; }

        public int Composite1Count { get; set; }

        public int Source5Count { get; set; }

        public int CompareTo(int other)
        {
            return this.Key.CompareTo(other);
        }

        public int GetKey()
        {
            return this.Key;
        }
    }
}
