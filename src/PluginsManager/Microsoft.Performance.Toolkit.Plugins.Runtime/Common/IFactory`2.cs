// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a factory that can create instances of <typeparamref name="T"/> using the given arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TArgs"></typeparam>
    public interface IFactory<out T, in TArgs>
    {
        /// <summary>
        ///     Creates an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="args">
        ///     The arguments to use when creating the instance.
        /// </param>
        /// <returns>
        ///     The created instance of <typeparamref name="T"/>.
        /// </returns>
        T Create(TArgs args);
    }
}
