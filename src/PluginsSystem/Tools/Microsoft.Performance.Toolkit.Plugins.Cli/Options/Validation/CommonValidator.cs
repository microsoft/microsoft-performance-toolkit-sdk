using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options.Validation
{
    internal abstract class CommonValidator<T, TResult> : IOptionsValidator<T, TResult>
        where T: PackageGenCommonOptions
        where TResult : TransformedBase
    {
        public abstract bool IsValid(T options, out TResult result);

        protected bool IsValidCommon(PackageGenCommonOptions options, out TransformedBase transformed)
        {
            // Validate source directory
            if (string.IsNullOrWhiteSpace(options.SourceDirectory))
            {
                throw new ArgumentValidationException("Source directory must be specified. Use --source <path> or -s <path>.");
            }

            if (!Directory.Exists(options.SourceDirectory))
            {
                throw new ArgumentValidationException($"Source directory '{options.SourceDirectory}' does not exist.");
            }

            var sourceDirectoryFullPath = Path.GetFullPath(options.SourceDirectory);

            string manifestFileFullPath = null;
            // Validate manifest file path
            if (options.ManifestFilePath != null)
            {
                if (!File.Exists(options.ManifestFilePath))
                {
                    throw new ArgumentValidationException($"Manifest file '{options.ManifestFilePath}' does not exist.");
                }

                manifestFileFullPath = Path.GetFullPath(options.ManifestFilePath);
            }

            transformed = new TransformedBase(sourceDirectoryFullPath, manifestFileFullPath, options.Overwrite);

            return true;
        }
    }
}
