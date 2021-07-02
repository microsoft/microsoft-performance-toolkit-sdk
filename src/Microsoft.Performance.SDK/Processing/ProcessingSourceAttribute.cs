// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     This attribute is used to mark a concrete class as an <see cref="IProcessingSource"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ProcessingSourceAttribute
        : CustomDataSourceAttribute
    {
        public ProcessingSourceAttribute(string guid, string name, string description) : base(guid, name, description)
        {
        }
    }
}
