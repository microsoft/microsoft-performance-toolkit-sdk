// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Composites
{
    public sealed class Composite2Cooker
        : CookedDataReflector,
          ICompositeDataCookerDescriptor
    {
        public static readonly DataCookerPath DataCookerPath = new DataCookerPath(nameof(Composite2Cooker));

        public Composite2Cooker()
            : base(DataCookerPath)
        {
            this.Output = new List<Composite2Output>();
        }

        public string Description => "Composite 2 cooker";

        public DataCookerPath Path => DataCookerPath;

        [DataOutput]
        public List<Composite2Output> Output { get; }

        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => new[]
        {
            Source5DataCooker.DataCookerPath,
        };

        public void OnDataAvailable(IDataExtensionRetrieval requiredData)
        {
            var data = requiredData.QueryOutput<List<Source5DataObject>>(
                new DataOutputPath(
                    Source5DataCooker.DataCookerPath,
                    nameof(Source5DataCooker.Objects)));

            this.Output.Add(
                new Composite2Output
                {
                    Key = data.Count,
                    Source5Count = data.Count,
                });
        }
    }

    public struct Composite2Output
        : IKeyedDataObject<int>
    {
        public int Key { get; set; }

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
