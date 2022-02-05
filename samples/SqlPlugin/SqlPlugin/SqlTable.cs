// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.ObjectModel;

namespace SqlPlugin
{
    //
    // Makes this table discoverable to SDK
    //
    [Table]
    public class SqlTable
    {
        private readonly ReadOnlyCollection<SqlEventWithRelativeTimestamp> sqlEvents;

        public SqlTable(ReadOnlyCollection<SqlEventWithRelativeTimestamp> content)
        {
            this.sqlEvents = content;
        }

        //
        // Exposes table information to SDK
        //
        public static TableDescriptor TableDescriptor => new TableDescriptor(
            Guid.Parse("{66B35410-42E3-4690-920B-FDB452A0DDCC}"),
            "SQL Trace Events",
            "SQL Trace recorded with SQL Server Profiler",
            "SQL");

        //
        // Column definitions
        //

        private static readonly ColumnConfiguration EventClassColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{0A8E1C32-F314-42FC-80C3-F60435BB364B}"), "EventClass", "The event class."),
            new UIHints { Width = 150 });

        private static readonly ColumnConfiguration TextDataColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{D10EE878-C898-4C2C-8536-DDC76F7005BD}"), "TextData", "The text data."),
            new UIHints { Width = 150 });

        private static readonly ColumnConfiguration ApplicationNameColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{4A5CFF5E-B733-4D70-8F73-FD307FA3A8BF}"), "ApplicationName", "The application name."),
            new UIHints { Width = 80 });

        private static readonly ColumnConfiguration NTUserNameColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{58C600B0-4AD8-4201-A46C-CD9E73E5F9E8}"), "NTUserName", "The NT username."),
            new UIHints { Width = 80 });

        private static readonly ColumnConfiguration LoginNameColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{A6440FE9-3CD7-40E1-818A-D15391BB333F}"), "LoginName", "The login name."),
            new UIHints { Width = 80 });

        private static readonly ColumnConfiguration CPUColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{67D56F61-23C4-44B8-8D2E-AB5A6F510785}"), "CPU", "The CPU."),
            new UIHints { Width = 80 });

        private static readonly ColumnConfiguration ReadsColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{94E4F964-297A-466F-9DB8-C03A8B544137}"), "Reads", "The reads."),
            new UIHints { Width = 40 });

        private static readonly ColumnConfiguration WritesColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{3BB79D1F-5AA4-4406-B7D3-A812A320B61A}"), "Writes", "The writes."),
            new UIHints { Width = 40 });

        private static readonly ColumnConfiguration DurationColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{578FE0B0-1D2F-47FB-BD33-CFFF533F2F58}"), "Duration", "The duration."),
            new UIHints { Width = 40 });

        private static readonly ColumnConfiguration ClientProcessIDColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{D8A66B2F-6184-408A-90F8-B7A333DDA915}"), "ClientProcessID", "The client process ID."),
            new UIHints { Width = 80 });

        private static readonly ColumnConfiguration SPIDColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{90B46DAE-0A7B-4137-A657-E57E33E437EF}"), "SPID", "The SPID."),
            new UIHints { Width = 40 });

        private static readonly ColumnConfiguration StartTimeColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{7EF895CD-AA02-4830-96B7-EC94A62BBDF5}"), "StartTime", "The start time."),
            new UIHints { Width = 80 });

        private static readonly ColumnConfiguration EndTimeColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{7B3A0BBB-BB4A-49BB-9273-B48CAA46CF73}"), "EndTime", "The end time."),
            new UIHints { Width = 80 });

        private static readonly ColumnConfiguration RelativeTimestampColumn = new ColumnConfiguration(
            new ColumnMetadata(new Guid("{AF511183-6E41-4CAB-8A09-109444533CF2}"), "Time", "The time relative to start of trace."),
            new UIHints { Width = 80 });

        internal void Build(ITableBuilder tableBuilder)
        {
            //
            // STEP 1: Create projections. For each column, we specify how to get data for a given row index
            //

            //
            // BASE PROJECTION: gets a SqlEvent from a row index. Every projection
            // below will use this projection
            //

            // SqlEventWithRelativeTimestamp eventAtIndex(int rowIndex) => this.sqlEvents[rowIndex];
            var baseProjection = Projection.Index(this.sqlEvents);

            //
            // COMPOSITE PROJECTIONS: once a SqlEvent is received from a row
            // index, map it to a speciifc property
            //

            // string eventClass(int rowIndex) => eventAtIndex(rowIndex).EventClass;
            // string eventClass(int rowIndex) => this.sqlEvents[rowIndex].EventClass;
            var eventClassProjection = baseProjection.Compose(x => x.EventClass);

            // string textData(int rowIndex) => eventAtIndex(rowIndex).TextData;
            // string textData(int rowIndex) => this.sqlEvents[rowIndex].TextData;
            var textDataProjection = baseProjection.Compose(x => x.TextData);

            // Etc
            var applicationNameProjection = baseProjection.Compose(x => x.ApplicationName);
            var ntUserNameProjection = baseProjection.Compose(x => x.NTUserName);
            var loginNameProjection = baseProjection.Compose(x => x.LoginName);
            var cpuProjection = baseProjection.Compose(x => x.CPU);
            var readsProjection = baseProjection.Compose(x => x.Reads);
            var writesProjection = baseProjection.Compose(x => x.Writes);
            var duratonProjection = baseProjection.Compose(x => x.Duration);
            var clientProcessIdProjection = baseProjection.Compose(x => x.ClientProcessId);
            var spidProjection = baseProjection.Compose(x => x.SPID);
            var startTimeProjection = baseProjection.Compose(x => x.StartTime);
            var endTimeProjection = baseProjection.Compose(x => x.EndTime);
            var relativeTimestampProjection = baseProjection.Compose(x => x.RelativeTimestamp);

            //
            // STEP 2: Create table configuration. We could have more than 1, but here we
            // only define 1 that includes all information.
            //
            
            //
            // When this configuration is applied, data will be grouped by the EventClassColumn (since that is
            // the only column before TableConfiguration.PivotColumn), and graphed by the RelativeTimestampColumn
            // (since that is the only column after TableConfiguration.GraphColumn).
            //
            var tableConfig = new TableConfiguration("SQL Trace Events by Event Class")
            {
                Columns = new[]
                {
                    EventClassColumn,
                    TableConfiguration.PivotColumn,
                    TableConfiguration.LeftFreezeColumn,
                    TextDataColumn,
                    ApplicationNameColumn,
                    NTUserNameColumn,
                    LoginNameColumn,
                    CPUColumn,
                    ReadsColumn,
                    WritesColumn,
                    DurationColumn,
                    ClientProcessIDColumn,
                    SPIDColumn,
                    StartTimeColumn,
                    EndTimeColumn,
                    TableConfiguration.RightFreezeColumn,
                    TableConfiguration.GraphColumn,
                    RelativeTimestampColumn
                }
            };

            //
            // Denote the RelativeTimestampColumn as the column which provides
            // a start time for graphing
            //
            tableConfig.AddColumnRole(ColumnRole.StartTime, RelativeTimestampColumn.Metadata.Guid);

            //
            // STEP 3: Build the table. This is where columns are actually "created"
            //
            tableBuilder.AddTableConfiguration(tableConfig)
                .SetDefaultTableConfiguration(tableConfig)
                .SetRowCount(this.sqlEvents.Count)
                .AddColumn(EventClassColumn, eventClassProjection)
                .AddColumn(TextDataColumn, textDataProjection)
                .AddColumn(ApplicationNameColumn, applicationNameProjection)
                .AddColumn(NTUserNameColumn, applicationNameProjection)
                .AddColumn(LoginNameColumn, loginNameProjection)
                .AddColumn(CPUColumn, cpuProjection)
                .AddColumn(ReadsColumn, readsProjection)
                .AddColumn(WritesColumn, writesProjection)
                .AddColumn(DurationColumn, duratonProjection)
                .AddColumn(ClientProcessIDColumn, clientProcessIdProjection)
                .AddColumn(SPIDColumn, spidProjection)
                .AddColumn(StartTimeColumn, startTimeProjection)
                .AddColumn(EndTimeColumn, endTimeProjection)
                .AddColumn(RelativeTimestampColumn, relativeTimestampProjection);
        }
    }
}
