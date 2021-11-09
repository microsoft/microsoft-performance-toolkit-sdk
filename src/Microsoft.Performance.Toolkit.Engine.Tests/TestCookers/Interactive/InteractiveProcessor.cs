// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Interactive
{
    public sealed class InteractiveProcessor
        : CustomDataProcessor
    {
        public InteractiveProcessor(
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment,
            IProcessorEnvironment processorEnvironment,
            IEnumerable<TableDescriptor> allTablesMapping)
            : base(options, applicationEnvironment, processorEnvironment, allTablesMapping)
        {
        }

        public override DataSourceInfo GetDataSourceInfo()
        {
            return DataSourceInfo.Default;
        }

        protected override void BuildTableCore(
            TableDescriptor tableDescriptor,
            ITableBuilder tableBuilder)
        {
        }

        protected override Task ProcessAsyncCore(
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var done = false;
            while (!done)
            {

                this.ApplicationEnvironment.DisplayMessage(
                    MessageType.Information,
                    CultureInfo.CurrentCulture,
                    "This should always display as there is no request for interaction.");

                var btn = this.ApplicationEnvironment.MessageBox(
                    MessageType.Information,
                    CultureInfo.CurrentCulture,
                    Buttons.OK,
                    "Test",
                    "Ask for OK");
                this.ApplicationEnvironment.Show("Received {0:G}", btn);

                btn = this.ApplicationEnvironment.MessageBox(
                    MessageType.Information,
                    CultureInfo.CurrentCulture,
                    Buttons.OKCancel,
                    "Test",
                    "Ask for OK / Cancel");
                this.ApplicationEnvironment.Show("Received {0:G}", btn);

                btn = this.ApplicationEnvironment.MessageBox(
                    MessageType.Information,
                    CultureInfo.CurrentCulture,
                    Buttons.YesNo,
                    "Test",
                    "Ask for Yes / No");
                this.ApplicationEnvironment.Show("Received {0:G}", btn);

                btn = this.ApplicationEnvironment.MessageBox(
                    MessageType.Information,
                    CultureInfo.CurrentCulture,
                    Buttons.YesNoCancel,
                    "Test",
                    "Ask for Yes / No / Cancel");
                this.ApplicationEnvironment.Show("Received {0:G}", btn);

                btn = this.ApplicationEnvironment.MessageBox(
                    MessageType.Information,
                    CultureInfo.CurrentCulture,
                    Buttons.YesNo,
                    "Test",
                    "Run input sequence again?");
                this.ApplicationEnvironment.Show("Received {0:G}", btn);
                done = btn == ButtonResult.No;
            }

            progress.Report(100);

            this.ApplicationEnvironment.Show("Input sequence complete. Processing complete.");

            return Task.CompletedTask;
        }
    }
}
