// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PlugInConfigurationEditor
{
    internal static class Logger
    {
        private static readonly Tuple<ConsoleColor, ConsoleColor> DefaultColors =
            Tuple.Create(Console.ForegroundColor, Console.BackgroundColor);

        // eventType to (foreground, background)
        private static readonly Dictionary<TraceEventType, Tuple<ConsoleColor, ConsoleColor>> ColorMap =
            new Dictionary<TraceEventType, Tuple<ConsoleColor, ConsoleColor>>
            {
                [TraceEventType.Critical] = Tuple.Create(ConsoleColor.DarkRed, ConsoleColor.Black),
                [TraceEventType.Error] = Tuple.Create(ConsoleColor.Red, ConsoleColor.Black),
                [TraceEventType.Information] = Tuple.Create(ConsoleColor.Gray, ConsoleColor.Black),
                [TraceEventType.Verbose] = Tuple.Create(ConsoleColor.DarkGray, ConsoleColor.Black),
                [TraceEventType.Warning] = Tuple.Create(ConsoleColor.Yellow, ConsoleColor.Black),

                [TraceEventType.Start] = Tuple.Create(ConsoleColor.DarkGreen, ConsoleColor.Black),
                [TraceEventType.Stop] = Tuple.Create(ConsoleColor.DarkGreen, ConsoleColor.Black),
                [TraceEventType.Resume] = Tuple.Create(ConsoleColor.Cyan, ConsoleColor.Black),
                [TraceEventType.Suspend] = Tuple.Create(ConsoleColor.DarkMagenta, ConsoleColor.Black),
            };

        private static readonly object SyncRoot = new object();
        private static string logPrefix;

        static Logger()
        {
            IsVerboseEnabled = false;
            logPrefix = string.Empty;
        }

        internal static bool IsVerboseEnabled { get; private set; }

        internal static void EnableVerbose()
        {
            IsVerboseEnabled = true;
            Verbose("Verbose logging enabled.");
        }

        internal static void SetLogName(string logName)
        {
            if (string.IsNullOrWhiteSpace(logName))
            {
                Logger.logPrefix = string.Empty;
            }
            else
            {
                Logger.logPrefix = $"{logName}: ";
            }
        }

        internal static void Verbose(string format, params object[] args)
        {
            if (!IsVerboseEnabled)
            {
                return;
            }

            Write(TraceEventType.Verbose, format, args);
        }

        internal static void Information(string format, params object[] args)
        {
            Write(TraceEventType.Information, format, args);
        }

        internal static void Warning(string format, params object[] args)
        {
            Write(TraceEventType.Warning, format, args);
        }

        internal static void Error(string format, params object[] args)
        {
            Write(TraceEventType.Error, format, args);
        }

        internal static void Fatal(string format, params object[] args)
        {
            Write(TraceEventType.Critical, format, args);
        }

        private static void Write(TraceEventType eventType, string format, params object[] args)
        {
            var colors = GetColor(eventType);

            lock (SyncRoot)
            {
                var fg = Console.ForegroundColor;
                var bg = Console.BackgroundColor;
                try
                {
                    Console.ForegroundColor = colors.Item1;
                    Console.BackgroundColor = colors.Item2;
                    Console.Out.Write($"{Logger.logPrefix} {eventType}: ");
                    Console.Out.WriteLine(format, args);
                }
                finally
                {
                    Console.ForegroundColor = fg;
                    Console.BackgroundColor = bg;
                }
            }
        }

        private static Tuple<ConsoleColor, ConsoleColor> GetColor(
            TraceEventType eventType)
        {
            if (!ColorMap.TryGetValue(eventType, out var colors))
            {
                colors = DefaultColors;
            }

            return colors;
        }
    }
}
