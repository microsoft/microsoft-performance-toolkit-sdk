// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Extensibility.SourceParsing
{
    /// <summary>
    ///     Implement this interface to describe a source parser for use in processing.
    /// </summary>
    public interface ISourceParserDescriptor
    {
        /// <summary>
        ///     Gets the ID of the source processor.
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Gets the Type that will be passed into a data extension for the source.
        /// </summary>
        Type DataElementType { get; }

        /// <summary>
        ///     Gets the type that provides context for processing data of type <see cref="DataElementType"/>.
        /// </summary>
        Type DataContextType { get; }

        /// <summary>
        ///     Gets the type that is used as a key for indexing <see cref="DataElementType"/> elements.
        /// </summary>
        Type DataKeyType { get; }

        /// <summary>
        ///     Gets the maximum number of times that this parse may run.
        ///     When a source must be parsed multiple times based on data cooker dependencies,
        ///     this establishes the maximum number of times the source may be parsed.
        ///     Set to <see cref="SourceParsingConstants.UnlimitedPassCount"/> for an unrestricted
        ///     number of passes.
        /// </summary>
        int MaxSourceParseCount { get; }
    }
}
