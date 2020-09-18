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
    public sealed class Source1DataCooker
        : BaseSourceDataCooker<Source123DataObject, EngineTestContext, int>
    {
        public static readonly DataCookerPath DataCookerPath = new DataCookerPath(
            nameof(Source123Parser),
            nameof(Source1DataCooker));

        public Source1DataCooker()
            : this(DataCookerPath)
        {
        }

        public Source1DataCooker(DataCookerPath dataCookerPath) 
            : base(dataCookerPath)
        {
            this.Objects = new List<Source1DataObject>();
        }

        public override string Description => string.Empty;

        public override ReadOnlyHashSet<int> DataKeys => new ReadOnlyHashSet<int>(new HashSet<int>(new[] { 1, }));

        [DataOutput]
        public List<Source1DataObject> Objects { get; }

        public override DataProcessingResult CookDataElement(
            Source123DataObject data, 
            EngineTestContext context, 
            CancellationToken cancellationToken)
        {
            this.Objects.Add(
                new Source1DataObject
                {
                    Id = data.Id,
                    Data = data.Data,
                });
            return DataProcessingResult.Processed;
        }
    }
}
