// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using System;

namespace Microsoft.Performance.Testing.SDK
{
    public class StubApplicationEnvironment
        : IApplicationEnvironment
    {
        public string ApplicationName { get; set; }

        public string RuntimeName { get; set; }

        public bool GraphicalUserEnvironment { get; set; }

        public ISerializer Serializer { get; set; }

        public ITableDataSynchronization TableDataSynchronizer { get; set; }

        public bool VerboseOutput { get; set; }

        public ISourceDataCookerFactoryRetrieval SourceDataCookerFactoryRetrieval { get; set; }

        public ISourceSessionFactory SourceSessionFactory { get; set; }

        public void DisplayMessage(MessageType messageType, IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public ButtonResult MessageBox(MessageType messageType, IFormatProvider formatProvider, Buttons buttons, string caption, string format, params object[] args)
        {
            return ButtonResult.None;
        }
    }
}
