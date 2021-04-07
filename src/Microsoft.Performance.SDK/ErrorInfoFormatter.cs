// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
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
                IFormatProvider formatProvider,
                bool suppressInitialIndent = false)
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
                var initialIndent = suppressInitialIndent
                    ? string.Empty
                    : indent;
                sb.AppendFormat(formatProvider, "{0}Code: {1}", initialIndent, info.Code).AppendLine()
                  .AppendFormat(formatProvider, "{0}Message: {1}", indent, info.Message);
                if (!string.IsNullOrWhiteSpace(info.Target))
                {
                    sb.AppendLine()
                      .AppendFormat(formatProvider, "{0}Target: {1}", indent, info.Target);
                }

                foreach (var property in info.GetType().GetProperties())
                {
                    if (property.Name == nameof(info.Code) ||
                        property.Name == nameof(info.Message) ||
                        property.Name == nameof(info.Target) ||
                        property.Name == nameof(info.Details))
                    {
                        continue;
                    }

                    var propValue = property.GetValue(info);
                    if (propValue is IEnumerable e)
                    {
                        sb.AppendLine()
                          .AppendFormat(formatProvider, "{0}{1}: ", indent, property.Name);
                        foreach (var o in e)
                        {
                            sb.AppendLine()
                              .AppendFormat("{0}{1}", indent + "    ", o);
                        }
                    }
                    else
                    {
                        sb.AppendLine()
                          .AppendFormat(formatProvider, "{0}{1}: {2}", indent, property.Name, propValue?.ToString() ?? string.Empty);
                    }
                }

                if (info.Details != null && info.Details.Length > 0)
                {
                    sb.AppendLine()
                      .AppendFormat(formatProvider, "{0}Details:", indent);
                   
                    for (var i = 0; i < info.Details.Length; ++i)
                    {
                        var detail = info.Details[i];
                        sb.AppendLine()
                          .AppendFormat("{0}  * ", indent, i + 1);
                        FormatHelper(level + 1, sb, detail, formatProvider, true);
                    }
                }
            }
        }
    }
}
