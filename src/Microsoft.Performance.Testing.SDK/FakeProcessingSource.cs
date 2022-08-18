// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;

namespace Microsoft.Performance.Testing.SDK
{
    [ProcessingSource("{0B48CF41-45DB-42C1-8B23-E568EF5F560E}", "Fake", "This is a test class.")]
    public sealed class FakeProcessingSource
        : IProcessingSource
    {
        public IEnumerable<TableDescriptor> DataTables => Enumerable.Empty<TableDescriptor>();

        public IEnumerable<TableDescriptor> MetadataTables => Enumerable.Empty<TableDescriptor>();

        public IEnumerable<Option> CommandLineOptions => Enumerable.Empty<Option>();


        public ICustomDataProcessor CreateProcessorReturnValue { get; set; }
        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            return this.CreateProcessorReturnValue;
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            return this.CreateProcessorReturnValue;
        }

        public ICustomDataProcessor CreateProcessor(IDataSourceGroup dataSourceGroup, IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            return this.CreateProcessorReturnValue;
        }

        public void DisposeProcessor(ICustomDataProcessor processor)
        {
        }

        public ProcessingSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }


        public List<IDataSource> IsDataSourceSupportedCalls { get; } = new List<IDataSource>();
        public Exception IsDataSourceSupportedError { get; set; }
        public Dictionary<IDataSource, bool> IsDataSourceSupportedReturnValue { get; } = new Dictionary<IDataSource, bool>();
        public bool IsDataSourceSupported(IDataSource dataSource)
        {
            this.IsDataSourceSupportedCalls.Add(dataSource);
            if (this.IsDataSourceSupportedError != null)
            {
                throw this.IsDataSourceSupportedError;
            }
            
            if (!this.IsDataSourceSupportedReturnValue.TryGetValue(dataSource, out var r))
            {
                return false;
            }

            return r;
        }

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}
