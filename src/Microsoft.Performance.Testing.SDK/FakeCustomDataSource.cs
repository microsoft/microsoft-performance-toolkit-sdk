// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public sealed class FakeCustomDataSource
        : ICustomDataSource
    {
        public IEnumerable<TableDescriptor> DataTables => throw new NotImplementedException();

        public IEnumerable<TableDescriptor> MetadataTables => throw new NotImplementedException();

        public IEnumerable<Option> CommandLineOptions => throw new NotImplementedException();

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public void DisposeProcessor(ICustomDataProcessor processor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public bool IsDataSourceSupported(IDataSource dataSource)
        {
            throw new NotImplementedException();
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
