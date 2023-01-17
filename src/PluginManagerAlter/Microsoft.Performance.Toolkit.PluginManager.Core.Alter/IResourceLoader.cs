// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter
{
    /// <summary>
    ///     Loads plugin resources.
    /// </summary>
    public interface IResourceLoader
    {
        /// <summary>
        ///     Tries to load <see cref="IPluginResource"/>s from the specified directory.
        /// </summary>
        /// <param name="directory">
        ///     The directoty to load resouces from.
        /// </param>
        /// <param name="resources">
        ///     The loaded resources.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="resources"/> are successfully loaded. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool TryLoad(string directory);

        bool Subscribe<T>(ResourceRepository<T> resourceRepository) where T : IPluginResource;
    }
}
