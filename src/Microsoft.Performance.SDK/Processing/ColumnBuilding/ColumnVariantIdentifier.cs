// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding
{
    public sealed class ColumnVariantIdentifier
    {
        public Guid Id { get; }
        public string Name { get; }

        public ColumnVariantIdentifier(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}