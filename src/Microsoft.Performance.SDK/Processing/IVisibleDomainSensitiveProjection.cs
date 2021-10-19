// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Specifies that the projection is sensitive to a currently visible
    ///     domain, either directly or indirectly.
    ///     <para/>
    ///     If the projection itself does not depend on a visible domain,
    ///     but is composed of projections that do, then the projection
    ///     should implement this interface and delegate to the composing
    ///     projections appropriately.
    /// </summary>
    /// <example>
    ///     A projection that maps a time range to its percent of the total
    ///     current visible time range would be sensitive to the visible domain,
    ///     since when the visible time-domain changes the percentage values must be
    ///     updated.
    /// </example>
    /// <remarks>
    ///     Currently, only <seealso cref="TimeRange"/>s are supported domains.
    /// </remarks>
    public interface IVisibleDomainSensitiveProjection
        : ICloneable
    {
        /// <summary>
        ///     Notifies this instance that the visible domain has been changed, and
        ///     that data should be recalculated if necessary based on the new
        ///     domain.
        /// </summary>
        /// <param name="visibleDomain">
        ///     The current visible domain.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the projection has changed due to the new visible domain,
        ///     <c>false</c> otherwise.
        /// </returns>
        bool NotifyVisibleDomainChanged(IVisibleDomainRegion visibleDomain);

        /// <summary>
        ///     Gets a value indicating whether this instance actually
        ///     depends on the visible domain.
        /// </summary>
        bool DependsOnVisibleDomain { get; }
    }
}
