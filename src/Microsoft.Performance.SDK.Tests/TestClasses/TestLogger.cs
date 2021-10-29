// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public class LoggerArgs
    {
        public LoggerArgs(string fmt, params object[] args)
            : this(null, fmt, args)
        {
        }

        public LoggerArgs(Exception e, string fmt, params object[] args)
        {
            this.StringFormat = fmt;
            this.Args = args;
            this.E = e;
        }

        public string StringFormat { get; set; }

        public object[] Args { get; set; }

        public Exception E { get; set; }
    }

    public class TestLogger
        : ILogger
    {
        public List<LoggerArgs> VerboseCalls = new List<LoggerArgs>();
        public List<LoggerArgs> InfoCalls = new List<LoggerArgs>();
        public List<LoggerArgs> WarnCalls = new List<LoggerArgs>();
        public List<LoggerArgs> ErrorCalls = new List<LoggerArgs>();
        public List<LoggerArgs> FatalCalls = new List<LoggerArgs>();

        public void Error(string fmt, params object[] args)
        {
            this.ErrorCalls.Add(new LoggerArgs(fmt, args));
        }

        public void Error(Exception e, string fmt, params object[] args)
        {
            this.ErrorCalls.Add(new LoggerArgs(e, fmt, args));
        }

        public void Fatal(string fmt, params object[] args)
        {
            this.FatalCalls.Add(new LoggerArgs(fmt, args));
        }

        public void Fatal(Exception e, string fmt, params object[] args)
        {
            this.FatalCalls.Add(new LoggerArgs(e, fmt, args));
        }

        public void Info(string fmt, params object[] args)
        {
            this.InfoCalls.Add(new LoggerArgs(fmt, args));
        }

        public void Info(Exception e, string fmt, params object[] args)
        {
            this.InfoCalls.Add(new LoggerArgs(e, fmt, args));
        }

        public void Verbose(string fmt, params object[] args)
        {
            this.VerboseCalls.Add(new LoggerArgs(fmt, args));
        }

        public void Verbose(Exception e, string fmt, params object[] args)
        {
            this.VerboseCalls.Add(new LoggerArgs(e, fmt, args));
        }

        public void Warn(string fmt, params object[] args)
        {
            this.WarnCalls.Add(new LoggerArgs(fmt, args));
        }

        public void Warn(Exception e, string fmt, params object[] args)
        {
            this.WarnCalls.Add(new LoggerArgs(e, fmt, args));
        }
    }
}
