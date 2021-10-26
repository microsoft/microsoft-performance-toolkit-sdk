// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents an equality comparison between <see cref="DiscoveredTable"/>
    ///     instances.
    /// </summary>
    public abstract class DiscoveredTableEqualityComparer
        : IEqualityComparer<DiscoveredTable>
    {
        /// <summary>
        ///     Compares instances of <see cref="DiscoveredTable"/>s for equality
        ///     by comparing their table descriptors.
        /// </summary>
        public static readonly IEqualityComparer<DiscoveredTable> ByTableDescriptor;

        /// <summary>
        ///     Initializes the static members of the <see cref="DiscoveredTableEqualityComparer"/>
        ///     class.
        /// </summary>
        static DiscoveredTableEqualityComparer()
        {
            ByTableDescriptor = new ByTableDescriptorEqualityComparer();
        }

        /// <inheritdoc />
        public abstract bool Equals(DiscoveredTable x, DiscoveredTable y);

        /// <inheritdoc />
        public abstract int GetHashCode(DiscoveredTable obj);

        private sealed class ByTableDescriptorEqualityComparer
            : DiscoveredTableEqualityComparer
        {
            public override bool Equals(DiscoveredTable x, DiscoveredTable y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                return x != null &&
                       y != null &&
                       x.Descriptor.Equals(y.Descriptor);
            }

            public override int GetHashCode(DiscoveredTable obj)
            {
                Guard.NotNull(obj, nameof(obj));

                return obj.Descriptor.GetHashCode();
            }
        }
    }
}
