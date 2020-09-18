// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Converts instances to <see cref="TimestampDelta"/>
    ///     instances.
    /// </summary>
    public sealed class TimestampDeltaConverter
        : TypeConverter,
          ICustomTypeConverter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimestampDeltaConverter"/>
        ///     class.
        /// </summary>
        public TimestampDeltaConverter()
        {
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
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string asString = value as string;
            if (asString != null)
            {
                return TimestampDelta.Parse(asString.Trim());
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public Type GetOutputType()
        {
            return typeof(TimestampDelta);
        }
    }
}
