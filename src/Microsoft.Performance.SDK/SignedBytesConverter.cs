// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Converts instances to <see cref="SignedBytes"/>
    ///     instances.
    /// </summary>
    public sealed class SignedBytesConverter
        : TypeConverter,
          ICustomTypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string asString = value as string;
            if (asString != null)
            {
                long parsedLong;
                int precision;
                BytesUnits units;
                if (TryParse(asString, out parsedLong, out units, out precision))
                {
                    return new SignedBytes(parsedLong);
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        ///     Attempts to parse the given string into a count of bytes.
        /// </summary>
        /// <param name="str">
        ///     The string to parse.
        /// </param>
        /// <param name="bytes">
        ///     The number of bytes.
        /// </param>
        /// <param name="units">
        ///     The units to use when interpreting the count of <paramref name="bytes"/>.
        /// </param>
        /// <param name="precision">
        ///     The precision.
        /// </param>
        /// <returns>
        ///     <c>true</c> if parsing was successful; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="str"/> is <c>null</c>.
        /// </exception>
        public static bool TryParse(string str, out long bytes, out BytesUnits units, out int precision)
        {
            Guard.NotNull(str, nameof(str));

            if (string.IsNullOrWhiteSpace(str))
            {
                bytes = 0;
                precision = 0;
                units = BytesUnits.Bytes;
                return false;
            }

            str = str.Trim();

            string numberText;
            BytesConverterUtils.SplitNumberAndUnits(str, BytesUnits.Bytes, out numberText, out units);

            decimal number;
            if (!decimal.TryParse(numberText, out number))
            {
                bytes = 0;
                precision = 0;
                units = BytesUnits.Bytes;
                return false;
            }

            precision = DecimalUtils.GetPrecision(number);

            decimal tempBytes = BytesConverterUtils.ConvertToBytes(number, units);

            bool roundResult = DecimalUtils.TryRoundToInt64(tempBytes, MidpointRounding.AwayFromZero, out bytes);
            return roundResult;
        }

        /// <summary>
        ///     Converts the given count of bytes to the given units.
        /// </summary>
        /// <param name="bytesValue">
        ///     The count of bytes.
        /// </param>
        /// <param name="units">
        ///     The units into which to convert the count.
        /// </param>
        /// <returns>
        ///     The number of bytes as expressed in <paramref name="units"/>.
        /// </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        ///     <paramref name="units"/> is not a valid member of the <see cref="BytesUnits"/>
        ///     enumeration.
        /// </exception>
        public static decimal ConvertToBytesUnit(long bytesValue, BytesUnits units)
        {
            switch (units)
            {
                case BytesUnits.Bytes:
                    return bytesValue;

                case BytesUnits.Kilobytes:
                    return Decimal.Divide(bytesValue, (decimal)1024d);

                case BytesUnits.Megabytes:
                    return Decimal.Divide(bytesValue, (decimal)1048576d);

                case BytesUnits.Gigabytes:
                    return Decimal.Divide(bytesValue, (decimal)1073741824d);

                default:
                    throw new InvalidEnumArgumentException("units", (int)units, typeof(BytesUnits));
            }
        }

        /// <inheritdoc />
        public Type GetOutputType()
        {
            return typeof(SignedBytes);
        }
    }
}
