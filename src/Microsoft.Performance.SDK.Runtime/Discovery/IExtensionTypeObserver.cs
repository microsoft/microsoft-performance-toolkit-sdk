// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Implement to take part in extension discovery. The implementation will need to be registered with an
    ///     <see cref="IExtensionTypeProvider"/>.
    /// </summary>
    public interface IExtensionTypeObserver
    {
        /// <summary>
        ///     Called for each type found in an <see cref="IExtensionTypeProvider"/>.
        /// </summary>
        /// <param name="type">
        ///     The type to analyze.
        /// </param>
        /// <param name="sourceName">
        ///     The source of the given type.
        /// </param>
        void ProcessType(Type type, string sourceName);

        /// <summary>
        ///     Called when type discovery is complete.
        /// </summary>
        void DiscoveryComplete();
    }
}
