// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Auth;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Options.Values;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public class TestApplicationEnvironment
        : IApplicationEnvironmentV3
    {
        public string ApplicationName { get; set; }

        public string RuntimeName { get; set; }

        public bool IsInteractive { get; set; }

        public ITableConfigurationsSerializer Serializer { get; set; }

        public ITableDataSynchronization TableDataSynchronizer { get; set; }

        public bool VerboseOutput { get; set; }

        public ISourceDataCookerFactoryRetrieval SourceDataCookerFactoryRetrieval { get; set; }

        public ISourceSessionFactory SourceSessionFactory { get; set; }

        public IReadOnlyDictionary<Guid, PluginOptionValue> PluginOptions { get; set; }

        public void DisplayMessage(MessageType messageType, IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public ButtonResult MessageBox(MessageType messageType, IFormatProvider formatProvider, Buttons buttons, string caption, string format, params object[] args)
        {
            return ButtonResult.None;
        }

        public bool TryGetPluginOption<T>(Guid optionGuid, out T option) where T : PluginOptionValue
        {
            foreach (var kvp in this.PluginOptions)
            {
                if (kvp.Key == optionGuid && kvp.Value is T asT)
                {
                    option = asT;
                    return true;
                }
            }

            option = null;
            return false;
        }

        public bool TryGetAuthProvider<TAuth, TResult>(out IAuthProvider<TAuth, TResult> provider)
            where TAuth : IAuthMethod<TResult>
        {
            provider = null;
            return false;
        }
    }
}
