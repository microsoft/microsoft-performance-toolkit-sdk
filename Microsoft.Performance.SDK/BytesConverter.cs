// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Converts instances of objects to <see cref="Bytes"/>, if
    ///     possible.
    /// </summary>
    public sealed class BytesConverter
        : TypeConverter,
          ICustomTypeConverter
    {
        /// <summary>
        ///     Attempts to parse the given <see cref="string"/> into a
        ///     quantity of bytes.
        /// </summary>
        /// <param name="str">
        ///     The <see cref="string"/> to parse.
        /// </param>
        /// <param name="bytes">
        ///     The quantity of bytes parsed.
        /// </param>
        /// <param name="units">
        ///     The units of the bytes parsed.
        /// </param>
        /// <param name="precision">
        ///     The precision of the parsing.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="str"/> was able to be parsed;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string str, out ulong bytes, out BytesUnits units, out int precision)
        {
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

            bool roundResult = DecimalUtils.TryRoundToUInt64(tempBytes, MidpointRounding.AwayFromZero, out bytes);
            return roundResult;
        }

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
                ulong parsedULong;
                int precision;
                BytesUnits units;
                if (TryParse(asString, out parsedULong, out units, out precision))
                {
                    return new Bytes(parsedULong);
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        ///     Converts the given raw quantity of bytes to a quantity of
        ///     bytes using the given unit.
        /// </summary>
        /// <param name="bytesValue">
        ///     The raw number of bytes.
        /// </param>
        /// <param name="units">
        ///     The units with which to represent the quantity specified by
        ///     <paramref name="bytesValue"/>.
        /// </param>
        /// <returns>
        ///     A quantity, in the given <paramref name="units"/>, representing the
        ///     given quantity of bytes.
        /// </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        ///     <paramref name="units"/> is not a valid member of the <see cref="BytesUnits"/>
        ///     enumeration.
        /// </exception>
        public static decimal ConvertToBytesUnit(ulong bytesValue, BytesUnits units)
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
            return typeof(Bytes);
        }
    }
}
