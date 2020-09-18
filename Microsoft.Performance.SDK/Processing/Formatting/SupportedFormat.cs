// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace Microsoft.Performance.SDK.Processing.Formatting
{
    /// <summary>
    ///     This describes a string format for an <see cref="IFormatProvider"/>.
    ///     Includes: the format itself, a description, and an optional unit value.
    /// </summary>
    public readonly struct SupportedFormat
    {
        private readonly string description;

        internal SupportedFormat(string format, string description)
        {
            this.Format = format;
            this.description = description;
            this.Units = null;
        }

        internal SupportedFormat(string format, string description, string units)
        {
            this.Format = format;
            this.description = description;
            this.Units = units;
        }

        /// <summary>
        ///     Gets the string format.
        /// </summary>
        public string Format { get; }

        /// <summary>
        ///     Gets a description of the string format.
        /// </summary>
        public string Description
        {
            get
            {
                if (this.Units == null)
                {
                    return this.description;
                }

                return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", this.description, this.Units);
            }
        }

        /// <summary>
        ///     Gets the unit specified for the string format, or <c>null</c>.
        /// </summary>
        public string Units { get; }
    }
}