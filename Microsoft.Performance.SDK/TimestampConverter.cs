// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Converts instances to <see cref="Timestamp"/>
    ///     instances.
    /// </summary>
    public sealed class TimestampConverter
        : TypeConverter,
          ICustomTypeConverter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimestampConverter"/>.
        /// </summary>
        public TimestampConverter()
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
                return Timestamp.Parse(asString.Trim());
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
            return typeof(Timestamp);
        }
    }
}
