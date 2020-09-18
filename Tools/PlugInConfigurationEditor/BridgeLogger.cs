// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace PlugInConfigurationEditor
{
    internal class BridgeLogger
        : ILogger
    {
        public void Verbose(string fmt, params object[] args)
        {
            Logger.Verbose(fmt, args);
        }

        public void Verbose(Exception e, string fmt, params object[] args)
        {
            Logger.Verbose(fmt, args);
            Logger.Verbose("Exception: {0}", e.Message);
        }

        public void Info(string fmt, params object[] args)
        {
            Logger.Information(fmt, args);
        }

        public void Info(Exception e, string fmt, params object[] args)
        {
            Logger.Information(fmt, args);
            Logger.Information("Exception: {0}", e.Message);
        }

        public void Warn(string fmt, params object[] args)
        {
            Logger.Warning(fmt, args);
        }

        public void Warn(Exception e, string fmt, params object[] args)
        {
            Logger.Warning(fmt, args);
            Logger.Warning("Exception: {0}", e.Message);
        }

        public void Error(string fmt, params object[] args)
        {
            Logger.Error(fmt, args);
        }

        public void Error(Exception e, string fmt, params object[] args)
        {
            Logger.Error(fmt, args);
            Logger.Error("Exception: {0}", e.Message);
        }

        public void Fatal(string fmt, params object[] args)
        {
            Logger.Fatal(fmt, args);
        }

        public void Fatal(Exception e, string fmt, params object[] args)
        {
            Logger.Fatal(fmt, args);
            Logger.Fatal("Exception: {0}", e.Message);
        }
    }
}
