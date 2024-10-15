namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

public sealed class NullColumnVariant
    : IColumnVariant
{
    private NullColumnVariant()
    {
    }

    public static NullColumnVariant Instance { get; } = NullColumnVariant.Instance;

    private bool Equals(NullColumnVariant other)
    {
        return true;
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is NullColumnVariant other && Equals(other);
    }

    public override int GetHashCode()
    {
        return 42;
    }

    public bool Equals(IColumnVariant other)
    {
        return ReferenceEquals(this, other) || other is NullColumnVariant otherT && Equals(otherT);
    }

    public void Accept(IColumnVariantsVisitor visitor)
    {
        visitor.Visit(this);
    }
}