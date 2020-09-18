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
    public sealed class Source2DataCooker
        : BaseSourceDataCooker<Source123DataObject, EngineTestContext, int>
    {
        public static readonly DataCookerPath DataCookerPath = new DataCookerPath(
            nameof(Source123Parser),
            nameof(Source2DataCooker));

        public Source2DataCooker()
            : this(DataCookerPath)
        {
        }

        public Source2DataCooker(DataCookerPath dataCookerPath) 
            : base(dataCookerPath)
        {
            this.Objects = new List<Source2DataObject>();
        }

        public override string Description => string.Empty;

        public override ReadOnlyHashSet<int> DataKeys => new ReadOnlyHashSet<int>(new HashSet<int>(new[] { 2, }));

        [DataOutput]
        public List<Source2DataObject> Objects { get; set; }

        public override DataProcessingResult CookDataElement(
            Source123DataObject data, 
            EngineTestContext context, 
            CancellationToken cancellationToken)
        {
            this.Objects.Add(
                new Source2DataObject
                {
                    Id = data.Id,
                    Data = data.Data,
                });
            return DataProcessingResult.Processed;
        }
    }
}
