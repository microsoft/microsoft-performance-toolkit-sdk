// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using Microsoft.Performance.SDK.Processing.Formatting;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    /// Format Provider for <see cref="Bytes"/>.
    /// <inheritdoc cref="IFormatProvider"/>
    /// </summary>
    [DefaultFormat("MB")]
    [SupportedFormat(1, "B", "Bytes", "B")]
    [SupportedFormat(2, "KB", "Kilobytes", "KB")]
    [SupportedFormat(3, "MB", "Megabytes", "MB")]
    [SupportedFormat(4, "GB", "Gigabytes", "GB")]
    public class BytesFormatProvider
        : IFormatProvider,
          ICustomFormatter
    {
        private static readonly BytesFormatProvider singleton = new BytesFormatProvider();

        public static BytesFormatProvider Singleton
        {
            get
            {
                return BytesFormatProvider.singleton;
            }
        }

        public object GetFormat(Type formatType)
        {
            return this;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
            {
                return string.Empty;
            }

            Decimal divisor;
            int fractionDigits;
            switch (format)
            {
                case "B":
                    divisor = 1;
                    fractionDigits = 0;
                    break;

                case "KB":
                    divisor = 1024m;
                    fractionDigits = 3;
                    break;

                case "MB":
                    divisor = 1024m * 1024m;
                    fractionDigits = 3;
                    break;

                case "GB":
                    divisor = 1024m * 1024m * 1024m;
                    fractionDigits = 3;
                    break;

                default:
                    throw new FormatException();
            }

            Decimal bytes;
            if (arg is Bytes)
            {
                bytes = Convert.ToDecimal(((Bytes)arg).TotalBytes, CultureInfo.InvariantCulture) / divisor;
            }
            else if(arg is SignedBytes)
            {
                bytes = Convert.ToDecimal(((SignedBytes)arg).TotalBytes, CultureInfo.InvariantCulture) / divisor;
            }
            else if(arg is LargeSignedBytes)
            {
                bytes = ((LargeSignedBytes)arg).TotalBytes / divisor;
            }
            else if (arg is Double && ((Double)arg).IsNonReal())
            {
                return arg.ToString();
            }
            else
            {
                //bytes = (arg is Int64Abs) ?
                //((Int64Abs)arg).Value :
                bytes = Convert.ToDecimal(arg, CultureInfo.InvariantCulture) / divisor;
            }            

            return bytes.ToString(
                "N" + fractionDigits,
                CultureInfo.CurrentCulture);
        }
    }

    /// <summary>
    /// Format Provider for <see cref="SignedBytes"/>.
    /// <inheritdoc cref="IFormatProvider"/>
    /// </summary>
    public sealed class SignedBytesFormatProvider : BytesFormatProvider { }

    /// <summary>
    /// Format Provider for <see cref="LargeSignedBytes"/>.
    /// <inheritdoc cref="IFormatProvider"/>
    /// </summary>
    public sealed class LargeSignedBytesFormatProvider : BytesFormatProvider { }
}
