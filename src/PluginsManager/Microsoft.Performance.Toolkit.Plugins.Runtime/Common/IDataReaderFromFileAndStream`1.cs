// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a data reader that can read from a file or a stream.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of data entity.
    /// </typeparam>
    public interface IDataReaderFromFileAndStream<TEntity>
        : IDataReader<Stream, TEntity>,
          IDataReaderAsync<Stream, TEntity>,
          IDataReader<FileInfo, TEntity>,
          IDataReaderAsync<FileInfo, TEntity>
    {
    }
}
