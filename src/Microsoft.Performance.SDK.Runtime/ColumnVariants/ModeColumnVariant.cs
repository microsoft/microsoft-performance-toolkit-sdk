using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants
{
    public sealed class ModeColumnVariant
        : IColumnVariant
    {
        public ModeColumnVariant(
            ColumnVariantIdentifier modeIdentifier,
            IDataColumn modeColumn,
            IColumnVariant subVariant)
        {
            ModeIdentifier = modeIdentifier;
            ModeColumn = modeColumn;
            SubVariant = subVariant;
        }

        public ColumnVariantIdentifier ModeIdentifier { get; }

        public IDataColumn ModeColumn { get; }

        public IColumnVariant SubVariant { get; }

        public void Accept(IColumnVariantsVisitor visitor)
        {
            visitor.Visit(this);
        }

        private bool Equals(ModeColumnVariant other)
        {
            return this.ModeIdentifier.Equals(other.ModeIdentifier)
                   && Equals(SubVariant, other.SubVariant);
        }

        public bool Equals(IColumnVariant other)
        {
            return ReferenceEquals(this, other) || other is ModeColumnVariant otherT && Equals(otherT);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is IColumnVariant other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.ModeIdentifier?.GetHashCode() ?? 0,
                this.SubVariant?.GetHashCode() ?? 0);
        }
    }
}