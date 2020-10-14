namespace DataExtensionsSample
{
    public class DataExtensionsSampleException : System.Exception
    {
        public DataExtensionsSampleException() { }
        public DataExtensionsSampleException(string message) : base(message) { }
        public DataExtensionsSampleException(string message, System.Exception inner) : base(message, inner) { }
        protected DataExtensionsSampleException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}