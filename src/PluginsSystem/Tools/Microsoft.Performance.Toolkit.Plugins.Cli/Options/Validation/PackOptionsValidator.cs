
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options.Validation
{
    internal class PackOptionsValidator
        : CommonValidator<PackOptions, TransformedPackOptions>
    {
        public override bool IsValid(PackOptions options, out TransformedPackOptions result)
        {
            result = null;
            
            if (!IsValidCommon(options, out TransformedBase resultBase))
            {
                return false;
            }

            if (options.OutputFilePath == null && options.Overwrite)
            {
                throw new ArgumentValidationException("Cannot overwrite output file when output file is not specified.");
            }

            string? outputFileFullPath = null;
            if (options.OutputFilePath != null)
            {
                if (!Path.GetExtension(options.OutputFilePath).Equals(PackageConstants.PluginPackageExtension, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentValidationException($"Output file must have extension '{PackageConstants.PluginPackageExtension}'.");
                }

                try
                {
                    outputFileFullPath = Path.GetFullPath(options.OutputFilePath);
                }
                catch (Exception ex)
                {
                    throw new ArgumentValidationException("Unable to get full path to output file.", ex);
                }

                string? outputDir = Path.GetDirectoryName(outputFileFullPath);
                if (!Directory.Exists(outputDir))
                {
                    throw new ArgumentValidationException($"The directory '{outputDir}' does not exist. Please provide a valid directory path or create the directory and try again.");
                }
            }

            result = new TransformedPackOptions(resultBase, outputFileFullPath);
            return true;
        }
    }
}
