// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime
{
    public static class ProcessingSourceConstants
    {
        /// <summary>
        ///     Default Custom Data Source Root Folder for loading plugins.
        /// </summary>
        /// <remarks>
        ///     This is "CustomDataSources" because processing sources were historically
        ///     called custom data sources. This folder name is unchanged to increase
        ///     backwards compatability of plugins.
        /// </remarks>
        public const string ProcessingSourceRootFolderName = "CustomDataSources";
    }
}
