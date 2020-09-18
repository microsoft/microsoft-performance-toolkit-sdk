// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Converts instances of objects into <see cref="Address"/> instances,
    ///     if possible.
    /// </summary>
    public sealed class AddressConverter
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
                ulong parsedULong;
                if (ulong.TryParse(asString.Trim(), System.Globalization.NumberStyles.HexNumber, culture, out parsedULong))
                {
                    return new Address(parsedULong);
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc />
        public Type GetOutputType()
        {
            return typeof(Address);
        }
    }
}
