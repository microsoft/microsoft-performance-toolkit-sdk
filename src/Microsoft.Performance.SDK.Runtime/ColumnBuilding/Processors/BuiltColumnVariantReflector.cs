// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;

/// <summary>
///     Exposes the <see cref="IColumnVariant"/> that was passed to the processor.
/// </summary>
internal class BuiltColumnVariantReflector
    : IColumnVariantsProcessor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BuiltColumnVariantReflector"/>
    /// </summary>
    public BuiltColumnVariantReflector()
    {
        this.Output = NullColumnVariant.Instance;
    }

    /// <summary>
    ///     The <see cref="IColumnVariant"/> that was last passed to this processor.
    /// </summary>
    public IColumnVariant Output { get; private set; }

    public void ProcessColumnVariants(IColumnVariant variants)
    {
        this.Output = variants;
    }
}