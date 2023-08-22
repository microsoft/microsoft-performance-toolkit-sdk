using CommandLine;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    internal abstract class PackageGenCommonOptions
    {
        protected PackageGenCommonOptions(
            string sourceDirectory,
            bool overwrite,
            string? manifestFilePath)
        {
            this.SourceDirectory = sourceDirectory;
            this.Overwrite = overwrite;
            this.ManifestFilePath = manifestFilePath;
        }

        [Option(
           's',
           "source",
           Required = true,
           HelpText = "The directory containing the plugin binaries.")]
        public string SourceDirectory { get; }

        [Option(
            'w',
            "overwrite",
            Required = false,
            HelpText = "Indicates that the destination file should be overwritten if it already exists.")]
        public bool Overwrite { get; } = false;

        [Option(
            'm',
            "manifest",
            Required = false,
            HelpText = "Path to the plugin manifest file. If not specified, the program will attempt to find the manifest in the source directory.")]
        public string? ManifestFilePath { get; }

        protected void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.SourceDirectory))
            {
                // throw new ProcessingException("Source directory must be specified.");
            }

            if (string.IsNullOrWhiteSpace(this.ManifestFilePath))
            {
                // throw new ProcessingException("Manifest file must be specified.");
            }
        }
    }
}
