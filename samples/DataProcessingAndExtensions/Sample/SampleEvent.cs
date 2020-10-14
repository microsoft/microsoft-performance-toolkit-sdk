using System;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;

namespace DataExtensionsSample
{
    /// <summary>
    /// This contains parsed data from an element in the data source.
    /// </summary>
    public class SampleEvent
        : IKeyedDataType<string>
    {
        /// <summary>
        /// Each sample event has a name to identify it.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This is the data that the parser doesn't understand. It it left for a source data cooker to interpret
        /// this data and convert it into a usable type.
        /// </summary>
        public byte[] Value { get; set; }

        /// <summary>
        /// Timestamp at which this event was recorded.
        /// </summary>
        public Timestamp Timestamp { get; set; }

        /// <summary>
        /// This is used by the runtime to distribute this event to source data cookers that are interested in
        /// receiving it.
        /// </summary>
        /// <returns>String value to identify this event type</returns>
        public string GetKey() => this.Name;

        /// <inheritdoc />
        public int CompareTo(string name)
        {
            return StringComparer.InvariantCulture.Compare(this.Name, name);
        }
    }
}
