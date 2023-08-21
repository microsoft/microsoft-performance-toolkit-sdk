// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.SDK;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public class PluginSourceFilesValidator
        : IPluginSourceFilesValidator
    {
        private readonly ILogger logger;

        public PluginSourceFilesValidator(ILogger<PluginSourceFilesValidator> logger)
        {
            this.logger = logger;
        }

        public bool Validate(
            string sourceDir,
            bool manifestShouldPresent,
            out ValidatedPluginDirectory? validatedPluginDirectory)
        {
            Guard.NotNull(sourceDir, nameof(sourceDir));
            validatedPluginDirectory = null;

            string fullPath = Path.GetFullPath(sourceDir);

            // Check if the directory exists
            if (!Directory.Exists(fullPath))
            {
                this.logger.LogError($"Directory does not exist: {fullPath}");
                return false;
            }

            int dllCount = 0;
            string? manifestFilePath = null;
            string sdkAssemblyName = "Microsoft.Performance.SDK";
            var searchedSdkDlls = new List<string>();
            Version? sdkVersion = null;
            long totalSize = 0;
            try
            {
                foreach (string file in Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(file).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        dllCount++;
                        if (Path.GetFileNameWithoutExtension(file).Equals(sdkAssemblyName, StringComparison.OrdinalIgnoreCase))
                        {
                            this.logger.LogError($"{sdkAssemblyName} should not present in the directory.");
                            return false;
                        }

                        // Check if the dll references SDK
                        try
                        {
                            var assembly = Assembly.LoadFrom(file);
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
                                    this.logger.LogError($"Mutiple versions of {sdkAssemblyName} are referenced in the plugin: {sdkVersion} and {curVersion}. Please ensure only one version is referenced.");
                                    return false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                    else if (Path.GetFileName(file).Equals(Constants.BundledManifestName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!manifestShouldPresent)
                        {
                            this.logger.LogWarning($"Directory contains {Constants.BundledManifestName} when it should not: {fullPath}. This file will be ignored.");
                        }
                        else if (manifestFilePath != null)
                        {
                            this.logger.LogError($"Directory contains multiple manifests: {manifestFilePath}, {file}. Only one manifest is allowed.");
                            return false;
                        }

                        manifestFilePath = file;
                    }

                    totalSize += new FileInfo(file).Length;
                }
            }
            catch (IOException ex)
            {
                this.logger.LogError($"IO exceptions occurred while processing directory: {fullPath}.");
                return false;
            }

            // Check if the directory contains any DLLs
            if (dllCount == 0)
            {
                this.logger.LogError($"Directory does not contain any DLLs: {fullPath}.");
                return false;
            }

            // Check if the directory contains the manifest file if it should
            if (manifestShouldPresent && manifestFilePath == null)
            {
                this.logger.LogError($"Directory does not contain {Constants.BundledManifestName} as expected: {fullPath}.");
                return false;
            }

            if (sdkVersion == null)
            {
                this.logger.LogError($"Invalid plugin: {sdkAssemblyName} is not referenced anywhere in the plugin.");
                return false;
            }

            validatedPluginDirectory = new ValidatedPluginDirectory()
            {
                FullPath = fullPath,
                SdkVersion = sdkVersion,
                ManifestFilePath = manifestFilePath,
                PluginSize = totalSize
            };

            return true;
        }
    }
}
