// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Provides a way to get a checksum for a given target.
    /// </summary>
    /// <typeparam name="TTarget">
    ///     The type of the target.
    /// </typeparam>
    public interface IChecksumProvider<TTarget>
    {
        /// <summary>
        ///     Gets the checksum for the given target.
        /// </summary>
        /// <param name="target">
        ///     The target to get the checksum for.
        /// </param>
        /// <returns>
        ///     The checksum for the given target.
        /// </returns>
        Task<string> GetChecksumAsync(TTarget target);
    }
}
