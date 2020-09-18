// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Performance.SDK
{
    internal sealed class ErrorInfoFormatter
        : ICustomFormatter
    {
        internal static readonly ICustomFormatter PlainText = new PlainTextErrorInfoFormatter();

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            switch (format)
            {
                case "G":
                    // PlainText is the default
                case null:
                    return PlainText.Format(format, arg, formatProvider);

                default:
                    throw new FormatException();
            }
        }

        private sealed class PlainTextErrorInfoFormatter
            : ICustomFormatter
        {
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                Debug.Assert(format == "G" || format == null);
                if (arg == null)
                {
                    return string.Empty;
                }

                var info = arg as ErrorInfo;
                Debug.Assert(info != null);

                var sb = new StringBuilder();

                FormatHelper(0, sb, info, formatProvider);

                return sb.ToString();
            }

            private static void FormatHelper(
                int level,
                StringBuilder sb,
                ErrorInfo info,
                IFormatProvider formatProvider)
            {
                Debug.Assert(level > -1);
                Debug.Assert(sb != null);

                if (info == null)
                {
                    return;
                }

                var indent = level > 0
                    ? new string(' ', 4 * level)
                    : string.Empty;
                sb.AppendFormat(formatProvider, "{0}Code: {1}", indent, info.Code).AppendLine()
                  .AppendFormat(formatProvider, "{0}Message: {1}", indent, info.Message);
                if (!string.IsNullOrWhiteSpace(info.Target))
                {
                    sb.AppendLine()
                      .AppendFormat(formatProvider, "{0}Target: {1}", indent, info.Target);
                }

                if (info.Details != null && info.Details.Length > 0)
                {
                    sb.AppendLine()
                      .AppendFormat(formatProvider, "{0}Details:", indent);
                    foreach (var detail in info.Details)
                    {
                        sb.AppendLine();
                        FormatHelper(level + 1, sb, detail, formatProvider);
                    }
                }

                if (info.Inner != null)
                {
                    sb.AppendLine()
                      .AppendFormat(formatProvider, "{0}Inner:", indent)
                      .AppendLine();
                    FormatHelper(level + 1, sb, info.Inner, formatProvider);
                }
            }

            private static void FormatHelper(
                int level,
                StringBuilder sb,
                InnerError info,
                IFormatProvider formatProvider)
            {
                Debug.Assert(level > -1);
                Debug.Assert(sb != null);

                if (info == null)
                {
                    return;
                }

                var indent = level > 0
                    ? new string(' ', 4 * level)
                    : string.Empty;
                sb.AppendFormat(formatProvider, "{0}Code: {1}", indent, info.Code);
                if (info.Inner != null)
                {
                    foreach (var property in info.Inner.GetType().GetProperties())
                    {
                        if (property.Name == nameof(InnerError.Code) ||
                            property.Name == nameof(InnerError.Inner))
                        {
                            continue;
                        }

                        sb.AppendLine()
                          .AppendFormat(formatProvider, "{0}{1}: {2}", indent, property.Name, property.GetValue(info.Inner));
                    }

                    sb.AppendLine()
                      .AppendFormat("{0}Inner:", indent).AppendLine();
                    FormatHelper(level + 1, sb, info.Inner, formatProvider);
                }
            }
        }
    }
}
