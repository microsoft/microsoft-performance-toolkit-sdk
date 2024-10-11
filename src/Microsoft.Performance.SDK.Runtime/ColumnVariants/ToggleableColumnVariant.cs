using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants
{
    public sealed class ToggleableColumnVariant
        : IColumnVariant
    {
        public ToggleableColumnVariant(
            ColumnVariantIdentifier identifier,
            IColumnVariant subVariant)
        {
            Identifier = identifier;
            SubVariant = subVariant;
        }

        public ColumnVariantIdentifier Identifier { get; }
        
        public IColumnVariant SubVariant { get; }
        
        public void Accept(IColumnVariantsVisitor visitor)
        {
            visitor.Visit(this);
        }

        private bool Equals(ToggleableColumnVariant other)
        {
            return this.Identifier.Equals(other.Identifier)
                   && Equals(SubVariant, other.SubVariant);
        }

        public bool Equals(IColumnVariant other)
        {
            return ReferenceEquals(this, other) || other is ToggleableColumnVariant otherT && Equals(otherT);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is IColumnVariant other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Identifier?.GetHashCode() ?? 0,
                this.SubVariant?.GetHashCode() ?? 0);
        }
    }
}