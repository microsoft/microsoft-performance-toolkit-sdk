// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a data writer that can write data to a target location.
    /// </summary>
    /// <typeparam name="TTarget">
    ///     The type of target location.
    /// </typeparam>
    /// <typeparam name="TEntity">
    ///     The type of data entity.
    /// </typeparam>
    public interface IDataWriter<TTarget, TEntity>
    {
        /// <summary>
        ///     Writes the given data entity to the given target location.
        /// </summary>
        /// <param name="target">
        ///     The target location to write to.
        /// </param>
        /// <param name="entity">
        ///     The data entity to write.
        /// </param>
        void WriteData(TTarget target, TEntity entity);
    }

}
