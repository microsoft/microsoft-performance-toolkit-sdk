using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    public class ColumnIdentifier
        : ICloneable<ColumnIdentifier>
    {
        public ColumnIdentifier(Guid columnGuid)
            : this(columnGuid, null)
        {
        }

        public ColumnIdentifier(ColumnIdentifier other)
        {
            this.ColumnGuid = other.ColumnGuid;
            this.VariantGuid = other.VariantGuid;
        }

        public ColumnIdentifier(Guid columnGuid, Guid? variantGuid)
        {
            ColumnGuid = columnGuid;
            this.VariantGuid = variantGuid;
        }

        public Guid ColumnGuid { get; }

        public Guid? VariantGuid { get; }

        public object Clone()
        {
            return new ColumnIdentifier(this);
        }

        /// <inheritdoc />
        public ColumnIdentifier CloneT()
        {
            return new ColumnIdentifier(this);
        }

        public override bool Equals(object obj)
        {
            return obj is ColumnIdentifier identifier &&
                   ColumnGuid.Equals(identifier.ColumnGuid) &&
                   EqualityComparer<Guid?>.Default.Equals(VariantGuid, identifier.VariantGuid);
        }

        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(this.ColumnGuid.GetHashCode(), this.VariantGuid?.GetHashCode() ?? 0);
        }
    }
}
