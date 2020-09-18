// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123
{
    public sealed class Source3DataCooker
        : BaseSourceDataCooker<Source123DataObject, EngineTestContext, int>
    {
        public static readonly DataCookerPath DataCookerPath = new DataCookerPath(
            nameof(Source123Parser),
            nameof(Source3DataCooker));
        private ICookedDataRetrieval dependencyRetrieval;

        public Source3DataCooker()
            : this(DataCookerPath)
        {
        }

        public Source3DataCooker(DataCookerPath dataCookerPath) 
            : base(dataCookerPath)
        {
            this.Objects = new List<Source3DataObject>();
            this.Source1Objects = new List<Source1DataObject>();
            this.Source2Objects = new List<Source2DataObject>();
        }

        public override string Description => string.Empty;

        public override ReadOnlyHashSet<int> DataKeys => new ReadOnlyHashSet<int>(new HashSet<int>(new[] { 3, }));

        public override IReadOnlyCollection<DataCookerPath> RequiredDataCookers => new[]
        {
            Source1DataCooker.DataCookerPath,
            Source2DataCooker.DataCookerPath,
        }.AsReadOnly();

        [DataOutput]
        public List<Source3DataObject> Objects { get; }

        [DataOutput]
        public List<Source1DataObject> Source1Objects { get; }

        [DataOutput]
        public List<Source2DataObject> Source2Objects { get; }

        public override void BeginDataCooking(
            ICookedDataRetrieval dependencyRetrieval, 
            CancellationToken cancellationToken)
        {
            base.BeginDataCooking(dependencyRetrieval, cancellationToken);
            this.dependencyRetrieval = dependencyRetrieval;
        }

        public override void EndDataCooking(CancellationToken cancellationToken)
        {
            base.EndDataCooking(cancellationToken);

            var source1Output = this.dependencyRetrieval.QueryOutput<List<Source1DataObject>>(
                new DataOutputPath(
                    Source1DataCooker.DataCookerPath,
                    nameof(Source1DataCooker.Objects)));

            var source2Output = this.dependencyRetrieval.QueryOutput<List<Source2DataObject>>(
                new DataOutputPath(
                    Source2DataCooker.DataCookerPath,
                    nameof(Source2DataCooker.Objects)));

            this.Source1Objects.AddRange(source1Output);
            this.Source2Objects.AddRange(source2Output);
        }

        public override DataProcessingResult CookDataElement(
            Source123DataObject data, 
            EngineTestContext context, 
            CancellationToken cancellationToken)
        {
            this.Objects.Add(
                new Source3DataObject
                {
                    Id = data.Id,
                    Data = data.Data,
                });
            return DataProcessingResult.Processed;
        }
    }
}
