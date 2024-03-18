// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Represents errors that occur when interacting with the WPT runtime.
    /// </summary>
    public class EngineException
        : Exception
    {
        /// <inheritdoc />
        public EngineException() : base() { }

        /// <inheritdoc />
        public EngineException(string message) : base(message) { }

        /// <inheritdoc />
        public EngineException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made to access a cooker
    ///     that is not known.
    /// </summary>
    public class CookerNotFoundException
        : EngineException
    {
        /// <inheritdoc />
        public CookerNotFoundException() : base() { }

        /// <inheritdoc />
        public CookerNotFoundException(string message) : base(message) { }

        /// <inheritdoc />
        public CookerNotFoundException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CookerNotFoundException"/>
        ///     class with the given cooker path.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     The path to the cooker that was not found.
        /// </param>
        public CookerNotFoundException(DataCookerPath dataCookerPath)
            : this(dataCookerPath, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CookerNotFoundException"/>
        ///     class with the given cooker path, and the error that is the cause of this error.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     The path to the cooker that was not found.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public CookerNotFoundException(DataCookerPath dataCookerPath, Exception inner)
            : base($"The requested cooker '{dataCookerPath}' was not found", inner)
        {
            this.DataCookerPath = dataCookerPath;
        }

        /// <summary>
        ///     Gets the requested cooker path that is the cause of this error.
        /// </summary>
        public DataCookerPath DataCookerPath { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(DataCookerPath), this.DataCookerPath)
                .ToString();
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made to access a
    ///     data output that is not known.
    /// </summary>
    public class DataOutputNotFoundException
        : EngineException
    {
        /// <inheritdoc />
        public DataOutputNotFoundException() : base() { }

        /// <inheritdoc />
        public DataOutputNotFoundException(string message) : base(message) { }

        /// <inheritdoc />
        public DataOutputNotFoundException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataOutputNotFoundException"/>
        ///     class with the given output path.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     The path to the data that was not found.
        /// </param>
        public DataOutputNotFoundException(DataOutputPath dataOutputPath)
            : this(dataOutputPath, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataOutputNotFoundException"/>
        ///     class with the given output path, and the error that is the cause of this error.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     The path to the data that was not found.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public DataOutputNotFoundException(DataOutputPath dataOutputPath, Exception inner)
            : base($"The requested data '{dataOutputPath}' was not found", inner)
        {
            this.DataOutputPath = dataOutputPath;
        }

        /// <summary>
        ///     Gets the requested data output path that is the cause of this error.
        /// </summary>
        public DataOutputPath DataOutputPath { get; }

        private DataCookerPath CookerPath => this.DataOutputPath.CookerPath;

        private string OutputId => this.DataOutputPath.OutputId;

        /// <inheritdoc />
        public override string ToString()
        {
            return new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(DataOutputPath), this.DataOutputPath)
                .ToString();
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made to access a
    ///     <see cref="TableDescriptor"/> that is not known.
    /// </summary>
    public class TableNotFoundException
        : EngineException
    {
        /// <inheritdoc />
        public TableNotFoundException() : base() { }

        /// <inheritdoc />
        public TableNotFoundException(string message) : base(message) { }

        /// <inheritdoc />
        public TableNotFoundException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableNotFoundException"/>
        ///     class with the given <see cref="TableDescriptor"/>.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The path to the data that was not found.
        /// </param>
        public TableNotFoundException(TableDescriptor tableDescriptor)
            : this(tableDescriptor, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableNotFoundException"/>
        ///     class with the given <see cref="TableDescriptor"/>, and the error that is the cause of this error.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The <see cref="TableDescriptor"/> that was not found.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public TableNotFoundException(TableDescriptor tableDescriptor, Exception inner)
            : base($"The requested table '{tableDescriptor}' was not found", inner)
        {
            this.Descriptor = tableDescriptor;
        }

        /// <summary>
        ///     Gets the requested <see cref="TableDescriptor"/> that is the cause of this error.
        /// </summary>
        public TableDescriptor Descriptor { get; }

        private Guid TableGuid => this.Descriptor.Guid;

        private string Name => this.Descriptor.Name;

        private string Description => this.Descriptor.Description;

        /// <inheritdoc />
        public override string ToString()
        {
            return new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(Descriptor), this.Descriptor)
                .ToString();
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made to build a
    ///     <see cref="ITableResult"/> that with a specified <see cref="TableDescriptor"/>.
    /// </summary>
    public class TableException
        : EngineException
    {
        /// <inheritdoc />
        public TableException() : base() { }

        /// <inheritdoc />
        public TableException(string message) : base(message) { }

        /// <inheritdoc />
        public TableException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableException"/>
        ///     class with the given <see cref="TableDescriptor"/>.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        /// <param name="tableDescriptor">
        ///     The path to the data that was not found.
        /// </param>
        public TableException(string message, TableDescriptor tableDescriptor)
            : this(message, tableDescriptor, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableException"/>
        ///     class with the given <see cref="TableDescriptor"/>, and the error that is the cause of this error.
        /// </summary>
        /// <param name="message">
        ///     The message that describes the error.
        /// </param>
        /// <param name="tableDescriptor">
        ///     The <see cref="TableDescriptor"/> that was not found.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public TableException(string message, TableDescriptor tableDescriptor, Exception inner)
            : base(message, inner)
        {
            this.Descriptor = tableDescriptor;
        }

        /// <summary>
        ///     Gets the requested <see cref="TableDescriptor"/> that is the cause of this error.
        /// </summary>
        public TableDescriptor Descriptor { get; }

        private Guid TableGuid => this.Descriptor.Guid;

        private string Name => this.Descriptor.Name;

        private string Description => this.Descriptor.Description;

        /// <inheritdoc />
        public override string ToString()
        {
            return new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(Descriptor), this.Descriptor)
                .ToString();
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made
    ///     to add a Data Source for processing that no processor can handle.
    ///     This can also happen if the user requests a processor to
    ///     process a Data Source that the processor cannot process.
    /// </summary>
    public class UnsupportedDataSourceException
        : EngineException
    {
        /// <inheritdoc />
        public UnsupportedDataSourceException() : base() { }

        /// <inheritdoc />
        public UnsupportedDataSourceException(string message) : base(message, null) { }

        /// <inheritdoc />
        public UnsupportedDataSourceException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedDataSourceException" />
        ///     class for the given Data Source.
        /// </summary>
        /// <param name="dataSource">
        ///     The unsupported Data Source.
        /// </param>
        public UnsupportedDataSourceException(IDataSource dataSource)
            : this($"Data Source '{dataSource}' cannot be processed by any known {nameof(IProcessingSource)}.")
        {
            this.DataSource = dataSource.Uri.ToString();
            this.RequestedProcessingSource = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedDataSourceException" />
        ///     class for the given Data Source and requested <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The unsupported Data Source.
        /// </param>
        /// <param name="requestedProcessingSource">
        ///     The <see cref="IProcessingSource"/> that was requested for the file.
        /// </param>
        public UnsupportedDataSourceException(IDataSource dataSource, Type requestedProcessingSource)
            : this(dataSource, requestedProcessingSource, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedDataSourceException" />
        ///     class for the given <see cref="IDataSource"/>, requested <see cref="IProcessingSource"/>,
        ///     and the error that is the cause of this error.
        /// </summary>
        /// <param name="dataSource">
        ///     The unsupported Data Source.
        /// </param>
        /// <param name="requestedProcessingSource">
        ///     The <see cref="IProcessingSource"/> that was requested for the Data Source.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public UnsupportedDataSourceException(IDataSource dataSource, Type requestedProcessingSource, Exception inner)
            : base($"Data Source '{dataSource}' cannot be processed by '{requestedProcessingSource}'", inner)
        {
            this.DataSource = dataSource.Uri.ToString();
            this.RequestedProcessingSource = requestedProcessingSource?.FullName ?? string.Empty;
        }

        /// <summary>
        ///     Gets the URI of the requested Data Source.
        /// </summary>
        public string DataSource { get; }

        /// <summary>
        ///     Gets the fully qualified type name of the <see cref="IProcessingSource"/> requested for
        ///     the <see cref="IDataSource"/>. This property may be null if no <see cref="IProcessingSource"/> was requested
        ///     for the <see cref="IDataSource"/>. See <see cref="Type.FullName"/>.
        /// </summary>
        public string RequestedProcessingSource { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(DataSource), this.DataSource);
            if (!string.IsNullOrWhiteSpace(this.RequestedProcessingSource))
            {
                sb = sb.AppendLine()
                    .AppendFormat("{0}: {1}", nameof(RequestedProcessingSource), this.RequestedProcessingSource);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made
    ///     to use an <see cref="IProcessingSource"/> that is unknown by the runtime.
    /// </summary>
    public class UnsupportedProcessingSourceException
        : EngineException
    {
        /// <inheritdoc />
        public UnsupportedProcessingSourceException() : base() { }

        /// <inheritdoc />
        public UnsupportedProcessingSourceException(string message) : base(message) { }

        /// <inheritdoc />
        public UnsupportedProcessingSourceException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedProcessingSourceException" />
        ///     class for the requested <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="requestedProcessingSource">
        ///     The <see cref="IProcessingSource"/> that was requested.
        /// </param>
        public UnsupportedProcessingSourceException(Type requestedProcessingSource)
            : this(requestedProcessingSource, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedDataSourceException" />
        ///     class for the requested data source and the error that is the
        ///     cause of this error.
        /// </summary>
        /// <param name="requestedProcessingSource">
        ///     The <see cref="IProcessingSource"/> that was requested.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public UnsupportedProcessingSourceException(Type requestedProcessingSource, Exception inner)
            : base($"Data source '{requestedProcessingSource}' is unknown", inner)
        {
            this.RequestedProcessingSource = requestedProcessingSource.FullName;
        }

        /// <summary>
        ///     Gets the fully qualified type name of the <see cref="IProcessingSource"/> requested for
        ///     the file. This property may be null if no <see cref="IProcessingSource"/> was requested
        ///     for the file. See <see cref="Type.FullName"/>.
        /// </summary>
        public string RequestedProcessingSource { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(RequestedProcessingSource), this.RequestedProcessingSource)
                .ToString();
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made
    ///     to use a path that is not valid as a directory for extensions.
    /// </summary>
    public class InvalidExtensionDirectoryException
        : EngineException
    {
        /// <inheritdoc />
        public InvalidExtensionDirectoryException() : base() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidExtensionDirectoryException" />
        ///     class for the given path.
        /// </summary>
        /// <param name="path">
        ///     The path that is invalid.
        /// </param>
        public InvalidExtensionDirectoryException(string path)
            : this(path, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedDataSourceException" />
        ///     class for the given file and the error that is the cause of this error.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the unsupported file.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public InvalidExtensionDirectoryException(string filePath, Exception inner)
            : base($"The given path is not valid as an extension directory.", inner)
        {
            this.Path = filePath;
        }

        /// <summary>
        ///     Gets the invalid path.
        /// </summary>
        public string Path { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(Path), this.Path)
                .ToString();
        }
    }

    /// <summary>
    ///     Represents errors that occur when attempting to mutate
    ///     the runtime after it has already been processed.
    /// </summary>
    public class InstanceAlreadyProcessedException
        : Exception
    {
        /// <inheritdoc />
        public InstanceAlreadyProcessedException() : base() { }

        /// <inheritdoc />
        public InstanceAlreadyProcessedException(string message) : base(message) { }

        /// <inheritdoc />
        public InstanceAlreadyProcessedException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made to enable a
    ///     source cooker on the engine when the data sources do not provide the
    ///     data required for said cooker.
    ///     <para />
    ///     An example of this condition would be when you attempt to add a cooker
    ///     that cooks events from an ETL file (Event Tracing for Windows trace file),
    ///     but no ETL files are present in the set of files to process.
    /// </summary>
    public class NoDataSourceException
        : EngineException
    {
        /// <inheritdoc />
        public NoDataSourceException() : base() { }

        /// <inheritdoc />
        public NoDataSourceException(string message) : base(message) { }

        /// <inheritdoc />
        public NoDataSourceException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoDataSourceException"/>
        ///     class with the specified <see cref="DataCookerPath"/>.
        /// </summary>
        /// <param name="cookerPath">
        ///     The path to the cooker which will not be able to participate in data
        ///     processing due to there being inadequate data in the data source set.
        /// </param>
        public NoDataSourceException(DataCookerPath cookerPath)
            : this(cookerPath, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoDataSourceException"/>
        ///     class with the specified <see cref="DataCookerPath"/> and the error
        ///     that is the cause of this error, if any.
        /// </summary>
        /// <param name="cookerPath">
        ///     The path to the cooker which will not be able to participate in data
        ///     processing due to there being inadequate data in the data source set.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public NoDataSourceException(DataCookerPath cookerPath, Exception inner)
            : base("No data sources produce input for the specified cooker.", inner)
        {
            this.CookerPath = cookerPath;
        }

        /// <summary>
        ///     Gets the path to the cooker that will not have any input.
        /// </summary>
        public DataCookerPath CookerPath { get; }
    }

    /// <summary>
    ///     Represents errors that occur when attempting to build a table that was not enabled.
    /// </summary>
    public class TableNotEnabledException
        : TableException
    {
        /// <inheritdoc />
        public TableNotEnabledException() : base() { }

        /// <inheritdoc />
        public TableNotEnabledException(string message) : base(message) { }

        /// <inheritdoc />
        public TableNotEnabledException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableNotEnabledException"/>
        ///     class for the given table.
        /// </summary>
        /// <param name="descriptor">
        ///     The descriptor of the table that was requested to be built, but was not
        ///     enabled for processing.
        /// </param>
        public TableNotEnabledException(TableDescriptor descriptor)
            : base("The given table is not enabled.", descriptor)
        {
        }
    }
}
