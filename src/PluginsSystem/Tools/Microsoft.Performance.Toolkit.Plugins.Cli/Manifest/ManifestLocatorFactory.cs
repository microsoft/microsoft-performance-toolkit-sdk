// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    internal class ManifestLocatorFactory
    : IManifestLocatorFactory
    {
        private readonly ILoggerFactory loggerFactory;

        public ManifestLocatorFactory(
            ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public IManifestLocator Create(PackGenCommonArgs args)
        {
            if (args.ManifestFileFullPath != null)
            {
                return new CommandLineManifestFileLocator(args.ManifestFileFullPath);
            }
            else
            {
                return new SourceDirectoryManifestLocator(args.SourceDirectoryFullPath, this.loggerFactory.CreateLogger<SourceDirectoryManifestLocator>());
            }
        }
    }
}
