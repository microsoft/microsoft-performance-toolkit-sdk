// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SimplePlugin
{
    public class WordDataCooker
        : SourceDataCooker<WordEvent, WordSourceParser, string>
    {
        private Dictionary<string, List<Tuple<Timestamp, string>>> lines;

        public WordDataCooker()
            : base(Constants.CookerPath)
        {
            this.lines = new Dictionary<string, List<Tuple<Timestamp, string>>>();
        }

        [DataOutput]
        public IReadOnlyDictionary<string, IReadOnlyList<Tuple<Timestamp, string>>> Lines =>
            this.lines.ToDictionary(x => x.Key, x => x.Value as IReadOnlyList<Tuple<Timestamp, string>>);

        public override SourceDataCookerOptions Options => SourceDataCookerOptions.ReceiveAllDataElements;

        public override string Description => "I store words.";

        public override ReadOnlyHashSet<string> DataKeys => new ReadOnlyHashSet<string>(new HashSet<string>());

        public override DataProcessingResult CookDataElement(WordEvent data, WordSourceParser context, CancellationToken cancellationToken)
        {
            if (!this.lines.TryGetValue(data.FilePath, out var value))
            {
                value = new List<Tuple<Timestamp, string>>();
                this.lines.Add(data.FilePath, value);
            }

            value.Add(Tuple.Create(data.Time, data.Word));

            return DataProcessingResult.Processed;
        }
    }
}
