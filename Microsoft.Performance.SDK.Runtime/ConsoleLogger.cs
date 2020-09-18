// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Utility helper class for creating <see cref="ILogger"/> instances.
    /// </summary>
    public static class ConsoleLogger
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="StreamLogger"/> : <see cref="ILogger"/> that logs to <see cref="Console.Error"/> for <see cref="Type"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     <see cref="Type"/> used to create the <see cref="StreamLogger"/>.
        /// </typeparam>
        /// <returns>
        ///     <inheritdoc cref="ConsoleLogger.Create(Type)"/>
        /// </returns>
        public static ILogger Create<T>()
        {
            return Create(typeof(T));
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="StreamLogger"/> : <see cref="ILogger"/> that logs to <see cref="Console.Error"/> for a particular <see cref="Type"/>.
        /// </summary>
        /// <param name="type">
        ///     <see cref="Type"/> used to create the <see cref="StreamLogger"/>.
        /// </param>
        /// <returns>
        ///     Returns a instance of <see cref="ILogger"/>.
        /// </returns>
        public static ILogger Create(Type type)
        {
            Guard.NotNull(type, nameof(type));
            return new StreamLogger(type, Console.Error);
        }
    }
}
