// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.Formatting
{
    /// <summary>
    ///     Declares a format that an <see cref="IFormatProvider"/> supports.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class SupportedFormatAttribute
        : Attribute
    {
        /// <summary>
        ///     Basic constructor doesn't require units.
        /// </summary>
        /// <param name="ordinal">
        ///     This provides ordering when multiple supported formats exist.
        /// </param>
        /// <param name="format">
        ///     The string format.
        /// </param>
        /// <param name="description">
        ///     A description of the format.
        /// </param>
        public SupportedFormatAttribute(int ordinal, string format, string description)
        {
            this.Ordinal = ordinal;
            this.SupportedFormat = new SupportedFormat(format, description);
        }

        /// <summary>
        ///     Complete constructor includes unit of measure.
        /// </summary>
        /// <param name="ordinal">
        ///     This provides ordering when multiple supported formats exist.
        /// </param>
        /// <param name="format">
        ///     The string format.
        /// </param>
        /// <param name="description">
        ///     A description of the format.
        /// </param>
        /// <param name="units">
        ///     A unit of measure for the format.
        /// </param>
        public SupportedFormatAttribute(int ordinal, string format, string description, string units)
        {
            this.Ordinal = ordinal;
            this.SupportedFormat = new SupportedFormat(format, description, units);
        }

        /// <summary>
        ///     Gets the ordinal used to order multiple <see cref="SupportedFormatAttribute"/>.
        /// </summary>
        internal int Ordinal { get; }

        internal SupportedFormat SupportedFormat { get; }
    }
}