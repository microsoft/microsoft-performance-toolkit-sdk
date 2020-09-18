// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    [CustomDataSource("{D481BCCC-E22F-494E-951A-F6E039B57C66}", "FakeCustomDataSource", "A fake for tests.")]
    [FileDataSource(Extension)]
    public sealed class FakeCustomDataSource
        : ICustomDataSource
    {
        public const string Extension = ".etl";

        public IEnumerable<TableDescriptor> DataTables => Enumerable.Empty<TableDescriptor>();

        public IEnumerable<TableDescriptor> MetadataTables => Enumerable.Empty<TableDescriptor>();

        public IEnumerable<Option> CommandLineOptions => new[]
        {
            FakeCustomDataSourceOptions.FakeOptionOne,
            FakeCustomDataSourceOptions.FakeOptionTwo,
            FakeCustomDataSourceOptions.FakeOptionThree,
        };

        public ICustomDataProcessor CreateProcessor(
            IDataSource dataSource,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(
            IEnumerable<IDataSource> dataSources,
            IProcessorEnvironment processorEnvironment,
            ProcessorOptions options)
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

        public bool IsFileSupported(string path)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(
                Extension,
                Path.GetExtension(path));

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
