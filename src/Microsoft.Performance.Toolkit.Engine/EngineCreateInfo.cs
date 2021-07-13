// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Specifies a set of values that are used when
    ///     you create a <see cref="Engine"/>.
    /// </summary>
    public sealed class EngineCreateInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EngineCreateInfo"/> class.
        /// </summary>
        /// <param name="extensionDirectory">
        ///     The extension directory from which the runtime instance is to load plugins.
        /// </param>
        /// <remarks>
        ///     If no extension directory is specified, the current directory is used.
        /// </remarks>
        /// <exception cref="InvalidExtensionDirectoryException">
        ///     Thrown if <paramref name="extensionDirectory"/> is an invalid directory path.
        /// </exception>
        public EngineCreateInfo(string extensionDirectory = null)
            : this(new string[] { extensionDirectory ?? Environment.CurrentDirectory })
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EngineCreateInfo"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown is <paramref name="extensionDirectories"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidExtensionDirectoryException">
        ///     Thrown if <paramref name="extensionDirectories"/> contains an invalid directory path.
        /// </exception>
        public EngineCreateInfo(IEnumerable<string> extensionDirectories)
        {
            Guard.NotNull(extensionDirectories, nameof(extensionDirectories));

            var directories = new List<string>();

            foreach (var directory in extensionDirectories)
            {
                if (string.IsNullOrWhiteSpace(directory))
                {
                    throw new InvalidExtensionDirectoryException(directory);
                }

                DirectoryInfo dirInfo;
                try
                {
                    dirInfo = new DirectoryInfo(directory);
                }
                catch (Exception e)
                {
                    throw new InvalidExtensionDirectoryException(directory, e);
                }

                if (!dirInfo.Exists)
                {
                    throw new InvalidExtensionDirectoryException(directory);
                }

                directories.Add(dirInfo.FullName);
            }

            this.ExtensionDirectories = directories.AsReadOnly();
        }

        /// <summary>
        ///     Gets the extension directories from
        ///     which the runtime instance is to load plugins.
        /// </summary>
        public IEnumerable<string> ExtensionDirectories { get; }

        /// <summary>
        ///     Gets or sets the <see cref="IAssemblyLoader"/> to
        ///     use for loading plugins. This property may be <c>null</c>.
        ///     A <c>null</c> value indicates to use the default loading
        ///     behavior.
        ///     <para/>
        ///     The vast majority of use cases will not need to use
        ///     this property. Changing the loading behavior is for
        ///     advanced scenarios.
        /// </summary>
        public IAssemblyLoader AssemblyLoader { get; set; }

        /// <summary>
        ///     Gets the <see cref="VersionChecker"/> to
        ///     use for loading plugins. This property may be <c>null</c>.
        ///     A <c>null</c> value indicates to use the default loading
        ///     behavior.
        /// </summary>
        public VersionChecker Versioning { get; internal set; }

        /// <summary>
        ///     Gets or sets the name of the runtime on which the application is built.
        /// </summary>
        /// <remarks>
        ///     Defauls to "Microsoft.Performance.Toolkit.Engine".
        /// </remarks>
        public string RuntimeName { get; set; } = "Microsoft.Performance.Toolkit.Engine";

        /// <summary>
        ///     Gets or sets the application name.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets a means of logging information.
        /// </summary>
        public ILogger Logger { get; set; }
    }
}
