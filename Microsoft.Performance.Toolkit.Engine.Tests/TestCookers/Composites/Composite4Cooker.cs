// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Composites
{
    public sealed class Composite4Cooker
        : CookedDataReflector,
          ICompositeDataCookerDescriptor
    {
        public static readonly DataCookerPath DataCookerPath = new DataCookerPath(nameof(Composite4Cooker));

        public Composite4Cooker()
            : base(DataCookerPath)
        {
            this.Output = new List<Composite4Output>();
        }

        public string Description => "Composite 4 cooker";

        public DataCookerPath Path => DataCookerPath;

        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => new[]
        {
            Composite1Cooker.DataCookerPath,
            Composite3Cooker.DataCookerPath,
        };

        [DataOutput]
        public List<Composite4Output> Output { get; }

        public void OnDataAvailable(IDataExtensionRetrieval requiredData)
        {
            var composite1Data = requiredData.QueryOutput<List<Composite1Output>>(
               new DataOutputPath(
                   Composite1Cooker.DataCookerPath,
                   nameof(Composite1Cooker.Output)));
            var composite3Data = requiredData.QueryOutput<List<Composite3Output>>(
                new DataOutputPath(
                    Composite3Cooker.DataCookerPath,
                    nameof(Composite3Cooker.Output)));

            this.Output.Add(
                new Composite4Output
                {
                    Key = composite1Data.Count + composite3Data.Count,
                    Composite1Count = composite1Data.Count,
                    Composite3Count = composite3Data.Count,
                });
        }
    }

    public struct Composite4Output
    : IKeyedDataObject<int>
    {
        public int Key { get; set; }

        public int Composite1Count { get; set; }

        public int Composite3Count { get; set; }

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
