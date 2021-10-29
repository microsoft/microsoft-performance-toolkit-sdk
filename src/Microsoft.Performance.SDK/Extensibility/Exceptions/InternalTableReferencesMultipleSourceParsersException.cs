// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Extensibility.Exceptions
{
    /// <summary>
    ///     An internal table may only reference data from a single source parser.
    ///     See <seealso cref="TableDescriptor.IsInternalTable"/>.
    /// </summary>
    public class InternalTableReferencesMultipleSourceParsersException
        : ExtensionTableException
    {
        /// <inheritdoc/>
        public InternalTableReferencesMultipleSourceParsersException(TableDescriptor table)
            : base(string.Format(
                "Internal table {0} {{{1}}} requires data from multiple source parsers.",
                table.Type?.FullName ?? string.Empty,
                table.Guid))
        {
        }
    }
}
