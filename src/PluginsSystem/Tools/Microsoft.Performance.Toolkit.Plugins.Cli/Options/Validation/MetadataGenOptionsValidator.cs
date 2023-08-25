using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options.Validation
{
    internal class MetadataGenOptionsValidator
        : CommonValidator<MetadataGenOptions, TransformedMetadataGenOptions>
    {
        public override bool IsValid(MetadataGenOptions options, out TransformedMetadataGenOptions result)
        {
            result = null;

            if (!IsValidCommon(options, out TransformedBase resultBase))
            {
                return false;
            }

            if (options.OutputDirectory == null && options.Overwrite)
            {
                throw new ArgumentValidationException("Cannot overwrite output directory when output directory is not specified.");
            }

            string? outputDirectoryFullPath = null;
            if (options.OutputDirectory != null)
            {
                if (!Directory.Exists(options.OutputDirectory))
                {
                    throw new ArgumentValidationException($"Output directory '{options.OutputDirectory}' does not exist. Please create it or omit the --output option to use the current directory.");
                }

                outputDirectoryFullPath = Path.GetFullPath(options.OutputDirectory);
            }

            result = new TransformedMetadataGenOptions(resultBase, outputDirectoryFullPath);
            return true;
        }
    }
}
