// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <remarks>
    ///     This class will be deleted prior to SDK v1.0.0 release candidate 1. It is
    ///     currently not sealed to maintain backwards compatability with plugins.
    /// </remarks>
    [Obsolete("CustomDataSourceAttribute will be renamed to ProcessingSourceAttribute by v1.0.0 release candidate 1.")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomDataSourceAttribute
        : ProcessingSourceAttribute
    {
        public CustomDataSourceAttribute(
            string guid,
            string name,
            string description)
            : base(guid, name, description)
        {
        }
    }
}
