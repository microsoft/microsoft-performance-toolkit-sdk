// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace SqlPluginWithProcessingPipeline
{
    public class SqlDataCooker
        : BaseSourceDataCooker<SqlEvent, SqlSourceParser, string>
    {
        // Backing field for this cooker's DataOutput
        private readonly List<SqlEventWithRelativeTimestamp> sqlEventsWithRelativeTimestamps;

        public SqlDataCooker()
            : base(SqlPluginConstants.CookerPath)
        {
            this.sqlEventsWithRelativeTimestamps = new List<SqlEventWithRelativeTimestamp>();
            this.SqlEventsWithRelativeTimestamps = 
                new ReadOnlyCollection<SqlEventWithRelativeTimestamp>(this.sqlEventsWithRelativeTimestamps);
        }

        //
        //  The data this cooker outputs. Tables or other cookers can query for this data
        //  via the SDK runtime
        //
        [DataOutput]
        public IReadOnlyList<SqlEventWithRelativeTimestamp> SqlEventsWithRelativeTimestamps { get; }

        public override string Description => "Adds relative timestamps to sql events";

        // Instructs runtime to only send events with the given keys this data cooker
        public override ReadOnlyHashSet<string> DataKeys => 
            new ReadOnlyHashSet<string>(new HashSet<string> { "SQL:BatchStarting",
                                                              "SQL:BatchCompleted" });

        public override DataProcessingResult CookDataElement(SqlEvent sqlEvent,
                                                             SqlSourceParser context,
                                                             CancellationToken cancellationToken)
        {
            // Use context to get trace start time from the source parser's DataSourceInfo
            var traceStartTime = context.StartWallClockUtc;

            var offset = sqlEvent.StartTime.Subtract(traceStartTime);
            var relativeTimestamp = Timestamp.FromNanoseconds(offset.Ticks * 100);

            // Cook the event
            var cookedEvent = new SqlEventWithRelativeTimestamp(sqlEvent, relativeTimestamp);

            // Store cooked element
            this.sqlEventsWithRelativeTimestamps.Add(cookedEvent);

            return DataProcessingResult.Processed;
        }
    }
}
