// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.SourceParsing
{
    /// <summary>
    ///     Contains constants relevant to parsing data sources.
    /// </summary>
    public static class SourceParsingConstants
    {
        /// <summary>
        ///     This may be set on <see cref="ISourceParserDescriptor.MaxSourceParseCount"/> to allow
        ///     as many passes through the source as necessary.
        /// </summary>
        public static readonly int UnlimitedPassCount = 0;
    }
}
