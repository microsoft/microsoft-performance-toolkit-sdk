// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// Attribute specifies an external resource of prebuilt table configurations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PrebuiltConfigurationsFilePathAttribute
        : Attribute
    {
        /// <summary>
        /// Creates an attribute with a given a file path
        /// </summary>
        /// <param name="resourcePath">
        /// Path to an embedded resource file containing serialized table configurations.
        /// </param>
        public PrebuiltConfigurationsFilePathAttribute(string resourcePath)
        {
            Guard.NotNullOrWhiteSpace(resourcePath, nameof(resourcePath));

            this.ExternalResourceFilePath = resourcePath;
        }

        /// <summary>
        /// Gets a path to a file containing serialized table configurations.
        /// </summary>
        public string ExternalResourceFilePath { get; }
    }
}
