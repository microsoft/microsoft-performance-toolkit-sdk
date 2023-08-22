namespace Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions
{
    public class ConsoleAppException
        : Exception
    {
        internal ConsoleAppException()
        {
        }

        internal ConsoleAppException(string message)
            : base(message)
        {
        }

        internal ConsoleAppException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
