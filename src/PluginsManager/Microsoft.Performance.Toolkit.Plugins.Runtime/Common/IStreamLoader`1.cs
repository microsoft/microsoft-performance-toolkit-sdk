// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a loader that can load stream from a data source.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The type of data source.
    /// </typeparam>
    public interface IStreamLoader<TSource>
        : IDataReader<TSource, Stream>
    {
    }
}
