using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;

namespace DataExtensionsSample.SourceDataCookers
{
    public class EventStatsCooker
        : SampleSourceDataCooker
    {
        public const string CookerId = "EventStats";
        public const string CookerPathAsString = CustomSourceParser.SourceId + "/" + CookerId;
        public static readonly DataCookerPath CookerPath = new DataCookerPath(CustomSourceParser.SourceId, CookerId);

        private Dictionary<string, ulong> eventCounts;

        public EventStatsCooker()
            : base(CookerId)
        {
            this.eventCounts = new Dictionary<string, ulong>();
            this.EventCounts = new ReadOnlyDictionary<string, ulong>(this.eventCounts);
        }

        /// <summary>
        /// The attribute on this public property will trigger CookedDataReflector to expose this
        /// data through the ICookedDataSet interface. This data can be accessed through an IDataRetrieval
        /// instance using the entire path to this element: "SampleParser/EventStats/EventCounts".
        /// <remarks>
        /// This data will only be available to data extensions that have marked this cooker as required.
        /// </remarks>
        /// </summary>
        [DataOutput]
        public IReadOnlyDictionary<string, ulong> EventCounts { get; private set; }

        public override string Description => "Reports the number of each type of event found in the data source.";

        /// <summary>
        /// This cooker needs to see all events. So rather than specifying specific event keys here,
        /// the "Options" property is overriden below to receive all events.
        /// </summary>
        public override ReadOnlyHashSet<string> DataKeys => new ReadOnlyHashSet<string>(new HashSet<string>());

        /// <summary>
        /// This data cooker receives all data elements.
        /// Use this option with caution: this is a perf hit as all events
        /// in the data source have to be passed.
        /// </summary>
        public override SourceDataCookerOptions Options => SourceDataCookerOptions.ReceiveAllDataElements;

        /// <summary>
        /// This is called for each data element this data cooker has registered to receive.
        /// In this case, that means every data element in the data source.
        /// </summary>
        public override DataProcessingResult CookDataElement(
            SampleEvent data, 
            ISampleEventContext context, 
            CancellationToken cancellationToken)
        {
            if(!this.eventCounts.ContainsKey(data.Name))
            {
                this.eventCounts[data.Name] = 1;
            }
            else
            {
                this.eventCounts[data.Name]++;
            }

            return DataProcessingResult.Processed;
        }
    }
}
