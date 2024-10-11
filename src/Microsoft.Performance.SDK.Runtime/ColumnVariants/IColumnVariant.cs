using System;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants
{
    public interface IColumnVariant
        : IEquatable<IColumnVariant>
    {
        void Accept(IColumnVariantsVisitor visitor);
    }
}