// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Specifies that the projection is sensitive to the time range
    ///     specified in the viewport, either directly or indirectly.
    ///     <para/>
    ///     If the projection itself does not depend on the viewport,
    ///     but is composed of projections that do, then the projection
    ///     should implement this interface and delegate to the composing
    ///     projections appropriately.
    /// </summary>
    public interface IViewportSensitiveProjection
        : ICloneable
    {
        /// <summary>
        ///     Notifies this instance that the viewport has been changed, and
        ///     that data should be recalculated if necessary based on the new
        ///     viewport.
        /// </summary>
        /// <param name="viewport">
        ///     The current viewport.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the projection has changed due to the new viewport,
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="viewport"/> is <c>null</c>.
        /// </exception>
        bool NotifyViewportChanged(IVisibleTableRegion viewport);

        /// <summary>
        ///     Gets a value indicating whether this instance actually
        ///     depends on the viewport.
        /// </summary>
        bool DependsOnViewport { get; }
    }
}
