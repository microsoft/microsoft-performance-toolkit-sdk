// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public class PluginSourceFilesValidator
        : IPluginSourceFilesValidator
    {
        private readonly ILogger logger;

        public PluginSourceFilesValidator(Func<Type, ILogger> loggerFactory)
        {
            this.logger = loggerFactory(typeof(PluginSourceFilesValidator));
        }

        public bool Validate(string sourceDir, bool manifestShouldPresent)
        {
            Guard.NotNull(sourceDir, nameof(sourceDir));

            // Check if the directory exists
            if (!Directory.Exists(sourceDir))
            {
                this.logger.Error($"Directory does not exist: {sourceDir}");
                return false;
            }

            // Check if the directory contains any DLLs
            if (!Directory.EnumerateFiles(sourceDir, "*.dll", SearchOption.AllDirectories).Any())
            {
                this.logger.Error($"Directory does not contain any DLLs: {sourceDir}.");
                return false;
            }

            // Check if the directory contains the manifest file if it should
            if (manifestShouldPresent)
            {
                if (!File.Exists(Path.Combine(sourceDir, Constants.BundledManifestName)))
                {
                    this.logger.Error($"Directory does not contain an bundled manifest: {Constants.BundledManifestName} as expected: {sourceDir}.");
                    return false;
                }
            }

            // Check if the directory contains the SDK assembly which should not be present
            string sdkAssemblyName = "Microsoft.Performance.SDK";
            string[] searchedFiles = Directory.GetFiles(sourceDir, $"{sdkAssemblyName}.dll", SearchOption.AllDirectories);
            if (searchedFiles.Length != 0)
            {
                this.logger.Error($"{sdkAssemblyName} should not present in the directory.");
                return false;
            }

            // Check if the directory contains multiple versions of the SDK assembly
            Version? sdkVersion = null;
            foreach (string dllFile in Directory.GetFiles(sourceDir, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllFile);
                    AssemblyName? sdkRef = assembly.GetReferencedAssemblies()
                        .FirstOrDefault(assemblyName => assemblyName.Name?.Equals(sdkAssemblyName, StringComparison.OrdinalIgnoreCase) == true);

                    // Check if the assembly references the target dependency
                    if (sdkRef != null)
                    {
                        Version? curVersion = sdkRef.Version;
                        if (sdkVersion == null)
                        {
                            sdkVersion = curVersion;
                        }
                        else if (sdkVersion != curVersion)
                        {
                            this.logger.Error($"Mutiple versions of {sdkAssemblyName} are referenced in the plugin: {sdkVersion} and {curVersion}. Please ensure only one version is referenced.");
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {Path.GetFileName(dllFile)}: {ex.Message}");
                }
            }
            
            return true;
        }
    }
}
