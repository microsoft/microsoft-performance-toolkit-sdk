// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Compares instances of <see cref="ColumnConfiguration"/>
    ///     for equality.
    /// </summary>
    public static class ColumnConfigurationEqualityComparer
    {
        /// <summary>
        ///     Gets the default <see cref="IEqualityComparer{T}"/> for
        ///     <see cref="ColumnConfiguration"/> instances.
        /// </summary>
        public static readonly IEqualityComparer<ColumnConfiguration> Default = new ByGuid();

        private sealed class ByGuid
            : IEqualityComparer<ColumnConfiguration>
        {
            public bool Equals(ColumnConfiguration x, ColumnConfiguration y)
            {
                if (x is null)
                {
                    return y is null;
                }

                if (y is null)
                {
                    return false;
                }

                Debug.Assert(x != null);
                Debug.Assert(y != null);

                return x.Metadata.Guid == y.Metadata.Guid;
            }

            public int GetHashCode(ColumnConfiguration obj)
            {
                Guard.NotNull(obj, nameof(obj));
                return obj.Metadata.Guid.GetHashCode();
            }
        }
    }
}
