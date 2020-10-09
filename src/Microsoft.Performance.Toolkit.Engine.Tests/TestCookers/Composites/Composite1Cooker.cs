// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Composites
{
    public sealed class Composite1Cooker
        : CookedDataReflector,
          ICompositeDataCookerDescriptor
    {
        public static readonly DataCookerPath DataCookerPath = new DataCookerPath(nameof(Composite1Cooker));

        public Composite1Cooker()
            : base(DataCookerPath)
        {
            this.Output = new List<Composite1Output>();
        }

        public string Description => "Composite 1 cooker";

        public DataCookerPath Path => DataCookerPath;

        [DataOutput]
        public List<Composite1Output> Output { get; }

        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => new[]
        {
            Source3DataCooker.DataCookerPath,
            Source4DataCooker.DataCookerPath,
        };

        public void OnDataAvailable(IDataExtensionRetrieval requiredData)
        {
            var source3Data = requiredData.QueryOutput<List<Source3DataObject>>(
                new DataOutputPath(
                    Source3DataCooker.DataCookerPath,
                    nameof(Source3DataCooker.Objects)));
            var source4Data = requiredData.QueryOutput<List<Source4DataObject>>(
                new DataOutputPath(
                    Source4DataCooker.DataCookerPath,
                    nameof(Source4DataCooker.Objects)));

            this.Output.Add(
                new Composite1Output
                {
                    Key = source3Data.Count + source4Data.Count,
                    Source3Count = source3Data.Count,
                    Source4Count = source4Data.Count,
                });
        }
    }

    public struct Composite1Output
        : IKeyedDataObject<int>
    {
        public int Key { get; set; }

        public int Source3Count { get; set; }

        public int Source4Count { get; set; }

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
