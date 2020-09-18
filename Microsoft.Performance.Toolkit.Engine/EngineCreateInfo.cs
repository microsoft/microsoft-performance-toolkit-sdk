// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Specifies a set of values that are used when
    ///     you create a <see cref="Engine"/>.
    /// </summary>
    public sealed class EngineCreateInfo
    {
        private string extensionDirectory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EngineCreateInfo"/> 
        ///     class.
        /// </summary>
        public EngineCreateInfo()
        {
            this.ExtensionDirectory = Environment.CurrentDirectory;
        }

        /// <summary>
        ///     Gets or sets the extension directory from
        ///     which the runtime instance is to load plugins.
        ///     By default, this value is the current working
        ///     directory. If you set it to <c>null</c>,
        ///     the current working directory will be used.
        /// </summary>
        public string ExtensionDirectory
        {
            get
            {
                return this.extensionDirectory;
            }
            set
            {
                var candidateValue = value ?? Environment.CurrentDirectory;

                DirectoryInfo dirInfo;
                try
                {
                    dirInfo = new DirectoryInfo(candidateValue);
                }
                catch (Exception e)
                {
                    throw new InvalidExtensionDirectoryException(candidateValue, e);
                }

                if (!dirInfo.Exists)
                {
                    throw new InvalidExtensionDirectoryException(value);
                }

                this.extensionDirectory = dirInfo.FullName;
            }
        }

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
        public IAssemblyLoader AssemblyLoader{ get; set;  }
    }
}
