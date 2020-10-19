// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// Attribute specifies an embedded resource of prebuilt table configurations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PrebuiltConfigurationsEmbeddedResourceAttribute
        : Attribute
    {
        /// <summary>
        /// Creates an attribute with a given embedded resource path
        /// </summary>
        /// <param name="resourcePath">
        /// Path to an embedded resource file containing serialized table configurations.
        /// </param>
        public PrebuiltConfigurationsEmbeddedResourceAttribute(string resourcePath)
        {
            Guard.NotNullOrWhiteSpace(resourcePath, nameof(resourcePath));

            this.EmbeddedResourcePath = resourcePath;
        }

        /// <summary>
        /// Gets a path to an embedded resource path containing serialized table configurations.
        /// </summary>
        public string EmbeddedResourcePath { get; }
    }
}
