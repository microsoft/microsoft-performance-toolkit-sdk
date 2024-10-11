namespace Microsoft.Performance.SDK.Runtime.ColumnVariants
{
    /// <summary>
    ///     Represents a toggle for enabling/disabling <see cref="ModesColumnVariant"/>.
    ///     This <see cref="IColumnVariant"/> has no associated projection.
    /// </summary>
    public sealed class ModesToggleColumnVariant
        : IColumnVariant
    {
        public string ToggleText { get; }

        public ModesColumnVariant Modes { get; }

        public void Accept(IColumnVariantsVisitor visitor)
        {
            visitor.Visit(this);
        }

        private bool Equals(ModesToggleColumnVariant other)
        {
            return ToggleText == other.ToggleText && Equals(Modes, other.Modes);
        }

        public bool Equals(IColumnVariant other)
        {
            return ReferenceEquals(this, other) || other is ModesToggleColumnVariant otherT && Equals(otherT);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is IColumnVariant other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.ToggleText?.GetHashCode() ?? 0,
                this.Modes?.GetHashCode() ?? 0);
        }
    }
}