// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [CustomDataSource("{CABDB99F-F182-457B-B0B4-AD3DD62272D8}", "One", "One")]
    [FileDataSource(".csv")]
    public sealed class CdsOne
        : ICustomDataSource
    {
        public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

        public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

        public IEnumerable<Option> CommandLineOptions => new Option[0];

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
        }

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsFileSupported(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }

    [CustomDataSource("{CBA22346-53DB-44C7-9039-2CC5FADC07C1}", "Two", "Two")]
    [FileDataSource(".xml")]
    public sealed class CdsTwo
        : ICustomDataSource
    {
        public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

        public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

        public IEnumerable<Option> CommandLineOptions => new Option[0];

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
        }

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsFileSupported(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }

    [CustomDataSource("{0E031D79-9760-42FA-9E20-B5A957006545}", "Three", "Three")]
    [FileDataSource(".json")]
    public sealed class CdsThree
        : ICustomDataSource
    {
        public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

        public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

        public IEnumerable<Option> CommandLineOptions => new Option[0];

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
        }

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsFileSupported(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }


    [CustomDataSource("{72028435-50C8-4045-AD46-2CD6304E5BF1}", "Four", "Four")]
    [FileDataSource(".json")]
    public sealed class CdsWithoutInterface
    {
    }

    [FileDataSource(".json")]
    public sealed class CdsWithoutCdsAttribute
        : ICustomDataSource
    {
        public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

        public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

        public IEnumerable<Option> CommandLineOptions => new Option[0];

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
        }

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsFileSupported(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }

    [CustomDataSource("{215A72DF-2FD6-4DA5-9F6E-5BD419EAC357}", "Five", "Five")]
    public sealed class CdsWithoutDataSourceAttribute
        : ICustomDataSource
    {
        public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

        public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

        public IEnumerable<Option> CommandLineOptions => new Option[0];

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
        }

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsFileSupported(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CdsWithNoAttributes
        : ICustomDataSource
    {
        public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

        public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

        public IEnumerable<Option> CommandLineOptions => new Option[0];

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
        }

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsFileSupported(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }

    [CustomDataSource("{2F7B5A59-6792-48B1-822C-8D5D5F5C6198}", "Six", "Six")]
    [FileDataSource(".json")]
    public sealed class CdsWithParameterizedConstructor
        : ICustomDataSource
    {
        public CdsWithParameterizedConstructor(string parameter)
        {
        }

        public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

        public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

        public IEnumerable<Option> CommandLineOptions => new Option[0];

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
        }

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsFileSupported(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }

    [CustomDataSource("{228F130B-CE96-4195-8EE5-54B9B5056F6B}", "Seven", "Seven")]
    [FileDataSource(".json")]
    public sealed class CdsWithParameterlessAndParameterizedConstructor
        : ICustomDataSource
    {
        public CdsWithParameterlessAndParameterizedConstructor()
        {
        }

        public CdsWithParameterlessAndParameterizedConstructor(string parameter)
        {
        }

        public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

        public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

        public IEnumerable<Option> CommandLineOptions => new Option[0];

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
        }

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsFileSupported(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }

    [CustomDataSource("{797D49BC-D0EC-4C8D-B64E-CFABE0707CFF}", "Eight", "Eight")]
    [FileDataSource(".json")]
    public sealed class CdsWithInaccessibleConstructor
        : ICustomDataSource
    {
        internal CdsWithInaccessibleConstructor()
        {
        }

        public IEnumerable<TableDescriptor> DataTables => new TableDescriptor[0];

        public IEnumerable<TableDescriptor> MetadataTables => new TableDescriptor[0];

        public IEnumerable<Option> CommandLineOptions => new Option[0];

        public void SetApplicationEnvironment(IApplicationEnvironment applicationEnvironment)
        {
        }

        public ICustomDataProcessor CreateProcessor(IDataSource dataSource, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public ICustomDataProcessor CreateProcessor(IEnumerable<IDataSource> dataSources, IProcessorEnvironment processorEnvironment, ProcessorOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetSerializationStream(SerializationSource source)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TableConfiguration> GetTableConfigurations(TableDescriptor descriptor)
        {
            throw new NotImplementedException();
        }

        public CustomDataSourceInfo GetAboutInfo()
        {
            throw new NotImplementedException();
        }

        public bool IsFileSupported(string path)
        {
            throw new NotImplementedException();
        }

        public void SetLogger(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}
