// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

// todo: this class needs to update to make it more consistent.
// Validate doesn't check for multiple SupportedFormats that match the DefaultFormat, but does check that at least one exists.
// DefaultSupportedFormat does the opposite.

namespace Microsoft.Performance.SDK.Processing.Formatting
{
    /// <summary>
    ///     Additional functionality for <see cref="IFormatProvider"/>.
    /// </summary>
    public static class IFormatProviderExtensions
    {
        private static readonly IEnumerable<SupportedFormat> NoSupportedFormats = Enumerable.Empty<SupportedFormat>();

        /// <summary>
        ///     Performs some basic validation on an IFormatProvider, including the existence of a default format and
        ///     supported formats.
        /// </summary>
        /// <param name="formatProvider">
        ///     The format provider to validate.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If the format provider cannot be validated, this exception is thrown with additional information.
        /// </exception>
        public static void Validate(this IFormatProvider formatProvider)
        {
            string defaultFormat = formatProvider.DefaultFormat();
            IEnumerable<SupportedFormat> supportedFormats = formatProvider.SupportedFormats();
            if (defaultFormat == null)
            {
                if (supportedFormats.Any())
                {
                    throw new ArgumentException("A format provider with supported formats must declare a default format");
                }

                return;
            }

            if (!supportedFormats.Any())
            {
                return;
            }

            string[] supportedFormatStrings = supportedFormats.Select(_ => _.Format).Distinct().ToArray();
            if (supportedFormats.Count() != supportedFormats.Count())
            {
                throw new ArgumentException("Duplicate supported format");
            }

            // todo: add a check that there's only one supported format that matches the default format

            if (!supportedFormatStrings.Any(_ => _ == defaultFormat))
            {
                throw new ArgumentException("Default format is unsupported");
            }

            var attributes = (SupportedFormatAttribute[])Attribute.GetCustomAttributes(formatProvider.GetType(), typeof(SupportedFormatAttribute));
            if (!attributes.OrderBy(_ => _.Ordinal).Select(_ => _.Ordinal)
                    .SequenceEqual(Enumerable.Range(1, attributes.Length)))
            {
                throw new ArgumentException("Please fix the ordinals of the SupportedFormatAttributes");
            }
        }

        /// <summary>
        ///     Determines which format to use from a format provider, preferring <paramref name="format"/>.
        /// </summary>
        /// <param name="formatProvider">
        ///     Target format provider.
        /// </param>
        /// <param name="format">
        ///     Preferred format.
        /// </param>
        /// <returns>
        ///     The preferred format if it exists in the provider, otherwise the default format. If
        ///     no format can be found, <c>null</c> is returned.
        /// </returns>
        public static string FormatToUse(this IFormatProvider formatProvider, string format)
        {
            if (formatProvider == null)
            {
                return format;
            }

            if (format != null)
            {
                IEnumerable<SupportedFormat> supportedFormats = formatProvider.SupportedFormats();
                if (supportedFormats.Any(_ => _.Format == format))
                {
                    return format;
                }
            }

            var attribute = (DefaultFormatAttribute)Attribute.GetCustomAttribute(formatProvider.GetType(), typeof(DefaultFormatAttribute));
            return (attribute != null) ? attribute.DefaultFormat : null;
        }

        /// <summary>
        ///     Returns the default format for the given provider.
        /// </summary>
        /// <param name="formatProvider">
        ///     Target format provider.
        /// </param>
        /// <returns>
        ///     The default format for the target provider, or <c>null</c> if no format exists.
        /// </returns>
        public static string DefaultFormat(this IFormatProvider formatProvider)
        {
            return formatProvider.FormatToUse(null);
        }

        /// <summary>
        ///     Returns the available formats from the target format provider.
        /// </summary>
        /// <param name="formatProvider">
        ///     Target format provider.
        /// </param>
        /// <returns>
        ///     An enumeration of <see cref="SupportedFormat"/>.
        /// </returns>
        public static IEnumerable<SupportedFormat> SupportedFormats(this IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                return IFormatProviderExtensions.NoSupportedFormats;
            }

            var attributes = (SupportedFormatAttribute[])Attribute.GetCustomAttributes(formatProvider.GetType(), typeof(SupportedFormatAttribute));
            if (attributes.Length == 0)
            {
                return IFormatProviderExtensions.NoSupportedFormats;
            }

            return attributes.OrderBy(_ => _.Ordinal).Select(_ => _.SupportedFormat);
        }

        /// <summary>
        ///     Finds and returns the <see cref="SupportedFormat"/> that matches the <see cref="DefaultFormat"/>
        ///     for the target format provider.
        /// </summary>
        /// <param name="formatProvider">
        ///     Target format provider.
        /// </param>
        /// <returns>
        ///     The <see cref="SupportedFormat"/> whose format matches the <see cref="DefaultFormat"/> for the
        ///     target format provider, or <c>default</c> if there isn't a match.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if there is more than one <see cref="SupportedFormat"/> that matches the
        ///     <see cref="DefaultFormat"/>.
        /// </exception>
        public static SupportedFormat DefaultSupportedFormat(this IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                return default;
            }

            IEnumerable<SupportedFormat> supportedFormats = formatProvider.SupportedFormats();
            if (!supportedFormats.Any())
            {
                return default;
            }

            string defaultFormat = formatProvider.DefaultFormat();
            if (defaultFormat == null)
            {
                return default;
            }

            return supportedFormats.SingleOrDefault(_ => _.Format == defaultFormat);
        }

        /// <summary>
        ///     Return the units from the <see cref="SupportedFormat"/> associated with the target format provider where
        ///     the specified <paramref name="format"/> matches the <see cref="SupportedFormat"/>.
        /// </summary>
        /// <param name="formatProvider">
        ///     Target format provider.
        /// </param>
        /// <param name="format">
        ///     Format of the <see cref="SupportedFormat"/> whose unit value to return.
        /// </param>
        /// <returns>
        ///     A unit value from a <see cref="SupportedFormat"/>, or <c>null</c>.
        /// </returns>
        public static string Units(this IFormatProvider formatProvider, string format)
        {
            if (formatProvider == null || format == null)
            {
                return null;
            }

            SupportedFormatAttribute attribute =
                ((SupportedFormatAttribute[])Attribute.GetCustomAttributes(formatProvider.GetType(),
                                              typeof(SupportedFormatAttribute)))
                    .SingleOrDefault(_ => _.SupportedFormat.Format == format);

            return (attribute != null) ? attribute.SupportedFormat.Units : null;
        }
    }
}
