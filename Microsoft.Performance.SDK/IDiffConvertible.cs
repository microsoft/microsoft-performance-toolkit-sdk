// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Marker interface for a column that is diffable.
    /// </summary>
    public interface IDiffConvertible
    {
    }

    /// <summary>
    ///     When column are being diffed, sometimes they need special handling to show the correct diffed values.
    ///     One example of this is diffing byte values, which are typically unsigned.  The diffed value can be negative,
    ///     which gets clamped to zero, if we don't convert the column to a signed bytes type.
    ///     This interface defines the target type for the conversion.
    /// </summary>
    public interface IDiffConvertible<T>
        : IDiffConvertible
    {
    }
}
