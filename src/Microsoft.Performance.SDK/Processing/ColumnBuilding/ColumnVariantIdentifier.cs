// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding
{
    public sealed class ColumnVariantIdentifier
    {
        public Guid Id { get; }

        private bool Equals(ColumnVariantIdentifier other)
        {
            return Id.Equals(other.Id) && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ColumnVariantIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Id.GetHashCode(),
                this.Name.GetHashCode());
        }

        public string Name { get; }

        public ColumnVariantIdentifier(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}