using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants
{
    public sealed class ModesColumnVariant
        : IColumnVariant
    {
        public ModesColumnVariant(
            IReadOnlyCollection<IColumnVariant> modes,
            int defaultModeIndex)
        {
            Modes = modes;
            DefaultModeIndex = defaultModeIndex;
        }

        public IReadOnlyCollection<IColumnVariant> Modes { get; }

        public int DefaultModeIndex { get; }
        
        public void Accept(IColumnVariantsVisitor visitor)
        {
            visitor.Visit(this);
        }

        private bool Equals(ModesColumnVariant other)
        {
            if (this.Modes == null)
            {
                return other.Modes == null;
            }

            if (other.Modes == null)
            {
                return false;
            }

            return Enumerable.SequenceEqual(this.Modes, other.Modes) &&
                   this.DefaultModeIndex == other.DefaultModeIndex;
        }

        public bool Equals(IColumnVariant other)
        {
            return ReferenceEquals(this, other) || other is ModesColumnVariant otherT && Equals(otherT);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is IColumnVariant other && Equals(other);
        }

        public override int GetHashCode()
        {
            int result = this.DefaultModeIndex.GetHashCode();

            if (this.Modes != null)
            {
                foreach (IColumnVariant mode in this.Modes)
                {
                    result = HashCodeUtils.CombineHashCodeValues(result, mode?.GetHashCode() ?? 0);
                }
            }

            return result;
        }
    }
}