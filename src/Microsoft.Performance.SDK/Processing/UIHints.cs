// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides hints to the caller for rendering columns.
    /// </summary>
    public sealed class UIHints
        : ICloneable<UIHints>
    {
        private static readonly UIHints DefaultInstance = new UIHints();

        /// <summary>
        ///     Initializes a new instance of the <see cref="UIHints"/>
        ///     class.
        /// </summary>
        public UIHints()
        {
            this.Width = 80;
            this.IsVisible = true;
            this.TextAlignment = TextAlignment.Left;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UIHints"/>
        ///     class from an existing instance.
        /// </summary>
        /// <param name="other">
        ///     The instance of which to make a copy.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public UIHints(UIHints other)
        {
            Guard.NotNull(other, nameof(other));

            this.Width = other.Width;
            this.IsVisible = other.IsVisible;
            this.TextAlignment = other.TextAlignment;
            this.CellFormat = other.CellFormat;
            this.SortPriority = other.SortPriority;
            this.SortOrder = other.SortOrder;
            this.AggregationMode = other.AggregationMode;
        }

        /// <summary>
        ///     Gets or sets the width to use for rendering the column.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the column should
        ///     be rendered at all.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        ///     Gets or sets the alignment that should be used for text in
        ///     the column.
        /// </summary>
        public TextAlignment TextAlignment { get; set; }

        /// <summary>
        ///     Gets or sets a composite format string describing how
        ///     to format cell data. See <see cref="ColumnFormats"/> for
        ///     some examples. This property may be <c>null</c>.
        /// </summary>
        public string CellFormat { get; set; }

        /// <summary>
        ///     Gets or sets the sorting rules to use when sorting data in
        ///     the column.
        /// </summary>
        public SortOrder SortOrder { get; set; }

        /// <summary>
        ///     Gets or sets the priority of this column when sorting
        ///     by multiple columns.
        /// </summary>
        public int SortPriority { get; set; }

        /// <summary>
        ///     Gets or sets a value that determines how data from the
        ///     column will be aggregated in the table when multiple rows
        ///     are collapsed.
        /// </summary>
        public AggregationMode AggregationMode { get; set; }

        /// <summary>
        ///     Gets an instance of <see cref="UIHints"/> representing
        ///     default values.
        /// </summary>
        /// <returns>
        ///     A new instance of <see cref="UIHints"/> representing default
        ///     values.
        /// </returns>
        public static UIHints Default() => DefaultInstance.CloneT();

        /// <inheritdoc />
        public object Clone()
        {
            return this.CloneT();
        }

        /// <inheritdoc />
        public UIHints CloneT()
        {
            return new UIHints(this);
        }
    }
}
