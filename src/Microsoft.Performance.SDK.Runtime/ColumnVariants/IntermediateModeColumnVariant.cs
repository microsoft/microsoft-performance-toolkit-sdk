namespace Microsoft.Performance.SDK.Runtime.ColumnVariants
{
    public sealed class IntermediateModeColumnVariant
        : IColumnVariant
    {
        public string ModeText { get; }

        public ModesColumnVariant Modes { get; }

        public void Accept(IColumnVariantsVisitor visitor)
        {
            visitor.Visit(this);
        }

        public bool Equals(IColumnVariant other)
        {
            return ReferenceEquals(this, other) || (other is IntermediateModeColumnVariant otherT && Equals(otherT));
        }

        private bool Equals(IntermediateModeColumnVariant other)
        {
            return ModeText == other.ModeText && Equals(Modes, other.Modes);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || (obj is IColumnVariant other && Equals(other));
        }

        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                ModeText?.GetHashCode() ?? 0,
                Modes?.GetHashCode() ?? 0);
        }
    }
}