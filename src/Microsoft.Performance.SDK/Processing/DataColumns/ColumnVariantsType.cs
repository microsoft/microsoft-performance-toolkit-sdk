// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing.DataColumns
{
    /// <summary>
    ///     Represents the type of every variant within a <see cref="IDataColumnVariants"/>.
    /// </summary>
    public enum ColumnVariantsType
    {
        /// <summary>
        ///     Each of the variants is individual toggles that can be independently applied or unapplied.
        /// </summary>
        Toggles,

        /// <summary>
        ///     Each of the variants is mutually exclusive modes where one and only one can be applied at a time.
        /// </summary>
        Modes,
    }
}