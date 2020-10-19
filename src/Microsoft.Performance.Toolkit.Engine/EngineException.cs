// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Represents errors that occur when interacting with the WPT runtime.
    /// </summary>
    [Serializable]
    public class EngineException
        : Exception
    {
        /// <inheritdoc />
        public EngineException() : base() { }

        /// <inheritdoc />
        public EngineException(string message) : base(message) { }

        /// <inheritdoc />
        public EngineException(string message, Exception inner) : base(message, inner) { }

        /// <inheritdoc />
        protected EngineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made to access a cooker
    ///     that is not known.
    /// </summary>
    [Serializable]
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

        /// <inheritdoc />
        protected CookerNotFoundException(SerializationInfo info, StreamingContext context)
        {
            this.DataCookerPath = new DataCookerPath(info.GetString(nameof(DataCookerPath)));
        }

        /// <summary>
        ///     Gets the requested cooker path that is the cause of this error.
        /// </summary>
        public DataCookerPath DataCookerPath { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(DataCookerPath), this.DataCookerPath.CookerPath);
        }

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
    [Serializable]
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

        /// <inheritdoc />
        protected DataOutputNotFoundException(SerializationInfo info, StreamingContext context)
        {
            this.DataOutputPath = new DataOutputPath(
                new DataCookerPath(info.GetString(nameof(CookerPath))),
                info.GetString(nameof(OutputId)));
        }

        /// <summary>
        ///     Gets the requested data output path that is the cause of this error.
        /// </summary>
        public DataOutputPath DataOutputPath { get; }

        private DataCookerPath CookerPath => this.DataOutputPath.CookerPath;

        private string OutputId => this.DataOutputPath.OutputId;

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(CookerPath), this.CookerPath);
            info.AddValue(nameof(OutputId), this.OutputId);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(DataOutputPath), this.DataOutputPath)
                .ToString();
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made
    ///     to add a file for processing that no processor can handle.
    ///     This can also happen if the user requests a processor to
    ///     process a file that it cannot process.
    /// </summary>
    [Serializable]
    public class UnsupportedFileException
        : EngineException
    {
        /// <inheritdoc />
        public UnsupportedFileException() : base() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedFileException" />
        ///     class for the given file.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the unsupported file.
        /// </param>
        public UnsupportedFileException(string filePath)
            : this(filePath, (Exception)null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedFileException" />
        ///     class for the given file and the error that is the cause of this error.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the unsupported file.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public UnsupportedFileException(string filePath, Exception inner)
            : base($"The given file is not supported by any known processors", inner)
        {
            this.FilePath = filePath;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedFileException" />
        ///     class for the given file and requested data source.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the unsupported file.
        /// </param>
        /// <param name="requestedDataSource">
        ///     The data source that was requested for the file.
        /// </param>
        public UnsupportedFileException(string filePath, Type requestedDataSource)
            : this(filePath, requestedDataSource, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedFileException" />
        ///     class for the given file, requested data source, and the error that is the
        ///     cause of this error.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the unsupported file.
        /// </param>
        /// <param name="requestedDataSource">
        ///     The data source that was requested for the file.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public UnsupportedFileException(string filePath, Type requestedDataSource, Exception inner)
            : base($"File '{filePath}' cannot be processed by '{requestedDataSource}'", inner)
        {
            this.FilePath = filePath;
            this.RequestedDataSource = requestedDataSource.FullName;
        }

        /// <inheritdoc />
        protected UnsupportedFileException(SerializationInfo info, StreamingContext context)
        {
            this.FilePath = info.GetString(nameof(FilePath));
            this.RequestedDataSource = info.GetString(nameof(RequestedDataSource));
        }

        /// <summary>
        ///     Gets the path to the file that is not supported.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     Gets the fully qualified type name of the data source requested for
        ///     the file. This property may be null if no data source was requested
        ///     for the file. See <see cref="Type.FullName"/>.
        /// </summary>
        public string RequestedDataSource { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(FilePath), this.FilePath);
            info.AddValue(nameof(RequestedDataSource), this.RequestedDataSource);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(FilePath), this.FilePath);
            if (!string.IsNullOrWhiteSpace(this.RequestedDataSource))
            {
                sb = sb.AppendLine()
                    .AppendFormat("{0}: {1}", nameof(RequestedDataSource), this.RequestedDataSource);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made
    ///     to use a data source that is unknown by the runtime.
    /// </summary>
    [Serializable]
    public class UnsupportedDataSourceException
        : EngineException
    {
        /// <inheritdoc />
        public UnsupportedDataSourceException() : base() { }

        /// <inheritdoc />
        public UnsupportedDataSourceException(string message) : base(message) { }

        /// <inheritdoc />
        public UnsupportedDataSourceException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedDataSourceException" />
        ///     class for the requested data source.
        /// </summary>
        /// <param name="requestedDataSource">
        ///     The data source that was requested.
        /// </param>
        public UnsupportedDataSourceException(Type requestedDataSource)
            : this(requestedDataSource, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnsupportedFileException" />
        ///     class for the requested data source and the error that is the
        ///     cause of this error.
        /// </summary>
        /// <param name="requestedDataSource">
        ///     The data source that was requested.
        /// </param>
        /// <param name="inner">
        ///     The error that is the cause of this error.
        /// </param>
        public UnsupportedDataSourceException(Type requestedDataSource, Exception inner)
            : base($"Data source '{requestedDataSource}' is unknown", inner)
        {
            this.RequestedDataSource = requestedDataSource.FullName;
        }

        /// <inheritdoc />
        protected UnsupportedDataSourceException(SerializationInfo info, StreamingContext context)
        {
            this.RequestedDataSource = info.GetString(nameof(RequestedDataSource));
        }

        /// <summary>
        ///     Gets the fully qualified type name of the data source requested for
        ///     the file. This property may be null if no data source was requested
        ///     for the file. See <see cref="Type.FullName"/>.
        /// </summary>
        public string RequestedDataSource { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(RequestedDataSource), this.RequestedDataSource);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return new StringBuilder(base.ToString()).AppendLine()
                .AppendFormat("{0}: {1}", nameof(RequestedDataSource), this.RequestedDataSource)
                .ToString();
        }
    }

    /// <summary>
    ///     Represents the error that occurs when an attempt is made
    ///     to use a path that is not valid as a directory for extensions.
    /// </summary>
    [Serializable]
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
        ///     Initializes a new instance of the <see cref="UnsupportedFileException" />
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

        /// <inheritdoc />
        protected InvalidExtensionDirectoryException(SerializationInfo info, StreamingContext context)
        {
            this.Path = info.GetString(nameof(Path));
        }

        /// <summary>
        ///     Gets the invalid path.
        /// </summary>
        public string Path { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Path), this.Path);
        }

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
    [Serializable]
    public class InstanceAlreadyProcessedException
        : Exception
    {
        /// <inheritdoc />
        public InstanceAlreadyProcessedException() : base() { }

        /// <inheritdoc />
        public InstanceAlreadyProcessedException(string message) : base(message) { }

        /// <inheritdoc />
        public InstanceAlreadyProcessedException(string message, Exception inner) : base(message, inner) { }

        /// <inheritdoc />
        protected InstanceAlreadyProcessedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
