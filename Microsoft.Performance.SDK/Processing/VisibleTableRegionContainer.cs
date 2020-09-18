// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     This class provides a level of indirection that
    ///     our struct projectors can use to make sure that
    ///     modifications to copies still get the viewport
    ///     to all instances.
    ///     If we set the viewport on a copy, the mutation is
    ///     lost. If we set the viewport on the container that
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
    internal sealed class VisibleTableRegionContainer
    {
        public IVisibleTableRegion VisibleTableRegion { get; set; }

        public TimeRange Viewport => this.VisibleTableRegion?.Viewport ?? default(TimeRange);
    }
}
