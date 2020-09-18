// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Broadcasts discovered types for registered <see cref="IExtensionTypeObserver"/> to process.
    /// </summary>
    public interface IExtensionTypeProvider
    {
        /// <summary>
        ///     Registers an observer to receive types.
        /// </summary>
        /// <param name="observer">
        ///     The observer to register.
        /// </param>
        void RegisterTypeConsumer(IExtensionTypeObserver observer);
    }
}
