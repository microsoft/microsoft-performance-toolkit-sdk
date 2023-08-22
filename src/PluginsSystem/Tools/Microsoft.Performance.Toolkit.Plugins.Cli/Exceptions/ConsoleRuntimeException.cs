namespace Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions
{
    internal class ConsoleRuntimeException
        : ConsoleAppException
    {
        internal ConsoleRuntimeException()
        {
        }

        internal ConsoleRuntimeException(string message)
            : base(message)
        {
        }

        internal ConsoleRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
