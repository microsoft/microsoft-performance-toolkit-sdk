namespace Microsoft.Performance.SDK.Landmarks
{
    // to consider:
    //  - Should we make this a template, where Data is a generic type?
    //    - If so, should we have a separate string Description?
    //  - Some landamarks might have additional properties - such as a TimeRange.

    public abstract class Landmark
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Landmark"/> class.
        /// </summary>
        /// <param name="timestamp">
        ///     The timestamp of the landmark.
        /// </param>
        /// <param name="data">
        ///     Data associated with the landmark.
        /// </param>
        protected Landmark(Timestamp timestamp, string data)
        {
            Timestamp = timestamp;
            Data = data;
        }

        /// <summary>
        ///     Gets the timestamp of the landmark.
        /// </summary>
        public Timestamp Timestamp { get; }

        /// <summary>
        ///     Gets the data associated with the landmark.
        /// </summary>
        public string Data { get; }
    }
}
