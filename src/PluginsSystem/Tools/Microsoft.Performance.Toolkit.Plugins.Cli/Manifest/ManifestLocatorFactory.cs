// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     A factory for creating <see cref="IManifestLocator"/> instances.
    /// </summary>
    internal class ManifestLocatorFactory
        : IManifestLocatorFactory
    {
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ManifestLocatorFactory"/>
        /// </summary>
        /// <param name="loggerFactory">
        ///     The logger factory to use.
        /// </param>
        public ManifestLocatorFactory(
            ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public IManifestLocator Create(PackGenCommonArgs args)
        {
            if (args.ManifestFileFullPath == null)
            {

                return new SourceDirectoryManifestLocator(args.SourceDirectoryFullPath, this.loggerFactory.CreateLogger<SourceDirectoryManifestLocator>());
            }

            return new CommandLineManifestFileLocator(args.ManifestFileFullPath);
        }
    }
}
