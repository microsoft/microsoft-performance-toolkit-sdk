// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4
{
    public sealed class Source4DataCooker
        : BaseSourceDataCooker<Source4DataObject, EngineTestContext, int>
    {
        public static readonly DataCookerPath DataCookerPath = new DataCookerPath(
            nameof(Source4Parser),
            nameof(Source4DataCooker));

        public Source4DataCooker()
            : this(DataCookerPath)
        {
        }

        public Source4DataCooker(DataCookerPath dataCookerPath) 
            : base(dataCookerPath)
        {
            this.Objects = new List<Source4DataObject>();
        }

        public override string Description => string.Empty;

        public override ReadOnlyHashSet<int> DataKeys => new ReadOnlyHashSet<int>(new HashSet<int>());

        public override SourceDataCookerOptions Options => SourceDataCookerOptions.ReceiveAllDataElements;

        [DataOutput]
        public List<Source4DataObject> Objects { get; }

        public override DataProcessingResult CookDataElement(
            Source4DataObject data, 
            EngineTestContext context, 
            CancellationToken cancellationToken)
        {
            this.Objects.Add(data);
            return DataProcessingResult.Processed;
        }
    }
}
