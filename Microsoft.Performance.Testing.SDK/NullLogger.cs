// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public class NullLogger
        : ILogger
    {
        public void Error(string fmt, params object[] args)
        {
        }

        public void Error(Exception e, string fmt, params object[] args)
        {
        }

        public void Fatal(string fmt, params object[] args)
        {
        }

        public void Fatal(Exception e, string fmt, params object[] args)
        {
        }

        public void Info(string fmt, params object[] args)
        {
        }

        public void Info(Exception e, string fmt, params object[] args)
        {
        }

        public void Verbose(string fmt, params object[] args)
        {
        }

        public void Verbose(Exception e, string fmt, params object[] args)
        {
        }

        public void Warn(string fmt, params object[] args)
        {
        }

        public void Warn(Exception e, string fmt, params object[] args)
        {
        }
    }
}
