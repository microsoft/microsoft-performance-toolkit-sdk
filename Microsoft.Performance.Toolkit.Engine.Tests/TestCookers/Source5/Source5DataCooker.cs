// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source5
{
    public sealed class Source5DataCooker
        : BaseSourceDataCooker<Source5DataObject, EngineTestContext, int>
    {
        public static readonly DataCookerPath DataCookerPath = new DataCookerPath(
            nameof(Source5Parser),
            nameof(Source5DataCooker));

        public Source5DataCooker()
            : this(DataCookerPath)
        {
        }

        public Source5DataCooker(DataCookerPath dataCookerPath) 
            : base(dataCookerPath)
        {
            this.Objects = new List<Source5DataObject>();
        }

        public override string Description => string.Empty;

        public override ReadOnlyHashSet<int> DataKeys => new ReadOnlyHashSet<int>(new HashSet<int>());

        public override SourceDataCookerOptions Options => SourceDataCookerOptions.ReceiveAllDataElements;

        [DataOutput]
        public List<Source5DataObject> Objects { get; }

        public override DataProcessingResult CookDataElement(
            Source5DataObject data, 
            EngineTestContext context, 
            CancellationToken cancellationToken)
        {
            this.Objects.Add(data);
            return DataProcessingResult.Processed;
        }
    }
}
