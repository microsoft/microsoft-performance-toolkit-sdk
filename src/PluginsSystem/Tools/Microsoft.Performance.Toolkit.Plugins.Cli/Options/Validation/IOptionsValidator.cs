namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options.Validation
{
    internal interface IOptionsValidator<TOptions, T>
        where TOptions : IOptions
    {
        bool IsValid(TOptions options, out T result);
    }
}
