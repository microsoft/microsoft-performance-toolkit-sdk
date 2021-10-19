// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     This class provides a level of indirection that
    ///     our struct projectors can use to make sure that
    ///     modifications to copies still give the visible domain
    ///     to all instances.
    ///     If we set the visible domain on a copy, the mutation is
    ///     lost. If we set the visible domain on the container that
    ///     is on a copy, then the original instance will also
    ///     see the update.
    ///     <para/>
    ///     The projectors are structs because the table engine
    ///     strips away everything and lines them up in memory.
    ///     When the projectors were reference types, the Garbage Collector would
    ///     sometimes work too hard and we had terrible performance
    ///     problems.
    ///     <para/>
    ///     This class is internal as it is an implementation detail
    ///     of projections.
    /// </summary>
    internal sealed class VisibleDomainRegionContainer
    {
        public IVisibleDomainRegion VisibleDomainRegion { get; set; }

        public TimeRange VisibleDomain => this.VisibleDomainRegion?.Domain ?? default(TimeRange);
    }
}
