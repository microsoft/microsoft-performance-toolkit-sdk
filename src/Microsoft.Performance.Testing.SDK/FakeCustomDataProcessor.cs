// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class FakeCustomDataProcessor
        : ICustomDataProcessor
    {
        public void BuildTable(TableDescriptor table, ITableBuilder tableBuilder)
        {
            throw new NotImplementedException();
        }

        public ITableService CreateTableService(TableDescriptor table)
        {
            throw new NotImplementedException();
        }

        public bool DoesTableHaveData(TableDescriptor table)
        {
            throw new NotImplementedException();
        }

        public void EnableTable(TableDescriptor tableDescriptor)
        {
            throw new NotImplementedException();
        }

        public DataSourceInfo GetDataSourceInfo()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableDescriptor> GetEnabledTables()
        {
            throw new NotImplementedException();
        }

        public Task ProcessAsync(IProgress<int> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryEnableTable(TableDescriptor tableDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}
