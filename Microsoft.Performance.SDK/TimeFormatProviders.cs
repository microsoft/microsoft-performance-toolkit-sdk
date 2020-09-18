// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing.Formatting;
using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    /// Abstract helper for Time based objects
    /// <see cref="Timestamp"/>, <see cref="TimestampDelta"/>, <see cref="TimeRange"/>.
    /// <inheritdoc cref="IFormatProvider"/>
    /// </summary>
    [DefaultFormat(TimestampFormatter.FormatSecondsGrouped)]
    [SupportedFormat(1, TimestampFormatter.FormatSecondsGrouped, "Seconds", "s")]
    [SupportedFormat(2, TimestampFormatter.FormatMillisecondsGrouped, "Milliseconds", "ms")]
    [SupportedFormat(3, TimestampFormatter.FormatMicrosecondsGrouped, "Microseconds", "\x00B5s")]
    [SupportedFormat(4, TimestampFormatter.FormatNanosecondsGrouped, "Nanoseconds", "ns")]
    public abstract class TimeFormatProvider
        : IFormatProvider,
          ICustomFormatter
    {
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
            else if (arg is TimeRange)
            {
                var timeRange = (TimeRange)arg;
                return TimestampFormatter.ToString(timeRange.Duration.ToNanoseconds, format, formatProvider);
            }
            else if (arg is TimestampDelta)
            {
                var timestampDelta = (TimestampDelta)arg;
                return TimestampFormatter.ToString(timestampDelta.ToNanoseconds, format, formatProvider);
            }
            else if (arg is Timestamp)
            {
                var timestamp = (Timestamp)arg;
                return TimestampFormatter.ToString(timestamp.ToNanoseconds, format, formatProvider);
            }
            else if (arg is double)
            {
                var timestamp = Timestamp.FromNanoseconds((double)arg);
                return TimestampFormatter.ToString(timestamp.ToNanoseconds, format, formatProvider);
            }

            return arg.ToString();
        }
    }

    /// <summary>
    /// Format Provider for <see cref="Timestamp"/>.
    /// <inheritdoc cref="IFormatProvider"/>
    /// </summary>
    public sealed class TimestampFormatProvider : TimeFormatProvider { }

    /// <summary>
    /// Format Provider for <see cref="TimestampDelta"/>.
    /// <inheritdoc cref="IFormatProvider"/>
    /// </summary>
    public sealed class TimestampDeltaFormatProvider : TimeFormatProvider { }

    /// <summary>
    /// Format Provider for <see cref="TimeRange"/>.
    /// <inheritdoc cref="IFormatProvider"/>
    /// </summary>
    public sealed class TimeRangeFormatProvider : TimeFormatProvider { }
}
