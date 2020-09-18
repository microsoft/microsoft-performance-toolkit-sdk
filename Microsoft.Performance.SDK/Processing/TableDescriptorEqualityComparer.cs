// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Used to compare <see cref="TableDescriptor"/> instances for equality.
    /// </summary>
    public abstract class TableDescriptorEqualityComparer
        : IEqualityComparer<TableDescriptor>
    {
        /// <summary>
        ///     Uses the <see cref="TableDescriptor.Guid"/> to compare for equality.
        /// </summary>
        public static readonly IEqualityComparer<TableDescriptor> Default = new ByGuidComparer();

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableDescriptorEqualityComparer"/>
        ///     class.
        /// </summary>
        protected TableDescriptorEqualityComparer()
        {
        }

        /// <inheritdoc />
        public abstract bool Equals(TableDescriptor x, TableDescriptor y);

        /// <inheritdoc />
        public abstract int GetHashCode(TableDescriptor obj);

        private sealed class ByGuidComparer
            : TableDescriptorEqualityComparer
        {
            public override bool Equals(TableDescriptor x, TableDescriptor y)
            {
                if (x is null)
                {
                    return y is null;
                }
                else if (y is null)
                {
                    return false;
                }
                else
                {
                    return x.Guid == y.Guid;
                }
            }

            public override int GetHashCode(TableDescriptor obj)
            {
                Guard.NotNull(obj, nameof(obj));

                return obj.Guid.GetHashCode();
            }
        }
    }
}
