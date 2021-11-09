// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class MockCustomDataProcessor
        : ICustomDataProcessor
    {
        public MockCustomDataProcessor()
        {
            this.BuildTableFailures = new Dictionary<TableDescriptor, Exception>();
            this.BuildTableCalls = new ConcurrentDictionary<TableDescriptor, List<ITableBuilder>>();
            this.EnableFailures = new Dictionary<TableDescriptor, Exception>();
            this.EnableTableCalls = new List<TableDescriptor>();
            this.DataSourceInfo = new DataSourceInfo(1, 2, DateTime.UnixEpoch);
            this.ProcessCalls = new List<Tuple<IProgress<int>, CancellationToken>>();
            this.IsDataAvailableReturnValue = true;
        }

        public Dictionary<TableDescriptor, Exception> BuildTableFailures { get; }
        public ConcurrentDictionary<TableDescriptor, List<ITableBuilder>> BuildTableCalls { get; }

        public void BuildTable(
            TableDescriptor table,
            ITableBuilder tableBuilder)
        {
            if (!this.BuildTableCalls.TryGetValue(table, out List<ITableBuilder> builders))
            {
                builders = new List<ITableBuilder>();
                this.BuildTableCalls[table] = builders;
            }

            builders.Add(tableBuilder);

            if (this.BuildTableFailures.TryGetValue(table, out Exception e))
            {
                throw e;
            }
        }

        public Dictionary<TableDescriptor, Exception> EnableFailures { get; }
        public List<TableDescriptor> EnableTableCalls { get; }

        public void EnableTable(TableDescriptor tableDescriptor)
        {
            this.EnableTableCalls.Add(tableDescriptor);

            if (this.EnableFailures.TryGetValue(tableDescriptor, out Exception e))
            {
                throw e;
            }
        }


        public bool TryEnableTable(
            TableDescriptor tableDescriptor)
        {
            try
            {
                EnableTable(tableDescriptor);
                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        public Exception DataSourceInfoFailure { get; set; }
        public DataSourceInfo DataSourceInfo { get; set; }
        public int GetDataSourceInfoCalls { get; set; }
        public DataSourceInfo GetDataSourceInfo()
        {
            ++this.GetDataSourceInfoCalls;
            if (this.DataSourceInfoFailure != null)
            {
                throw this.DataSourceInfoFailure;
            }

            return this.DataSourceInfo;
        }

        public Exception ProcessFailure { get; set; }
        public List<Tuple<IProgress<int>, CancellationToken>> ProcessCalls { get; }
        public Task ProcessAsync(
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            this.ProcessCalls.Add(
                Tuple.Create(
                    progress,
                    cancellationToken));

            if (this.ProcessFailure != null)
            {
                throw this.ProcessFailure;
            }

            return Task.CompletedTask;
        }

        public bool IsDataAvailableReturnValue { get; set; }
        public bool DoesTableHaveData(TableDescriptor table)
        {
            return this.IsDataAvailableReturnValue;
        }

        public ITableService CreateTableService(TableDescriptor table)
        {
            return null;
        }

        public IEnumerable<TableDescriptor> GetEnabledTables()
        {
            throw new NotImplementedException();
        }
    }
}
