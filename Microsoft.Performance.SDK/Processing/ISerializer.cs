// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Used to perform serialization operations against <see cref="TableConfiguration"/>
    ///     instances.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        ///     Deserializes the content of a <see cref="Stream"/> into a collection
        ///     of <see cref="TableConfigurations"/> instances.
        /// </summary>
        /// <param name="stream">
        ///     The <see cref="Stream"/> from which to deserialize.
        /// </param>
        /// <returns>
        ///     The deserialized configurations.
        /// </returns>
        IEnumerable<TableConfigurations> DeserializeTableConfigurations(Stream stream);

        /// <summary>
        ///     Deserializes the content of a <see cref="Stream"/> into a collection
        ///     of <see cref="TableConfigurations"/> instances.
        /// </summary>
        /// <param name="stream">
        ///     The <see cref="Stream"/> from which to deserialize.
        /// </param>
        /// <param name="logger">
        ///     Used to log any relevant messages.
        /// </param>
        /// <returns>
        ///     The deserialized configurations.
        /// </returns>
        IEnumerable<TableConfigurations> DeserializeTableConfigurations(Stream stream, ILogger logger);
    }
}
