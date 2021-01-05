// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Testing.SDK
{
    public class FakeCustomDataSourceReference
        : CustomDataSourceReference
    {
        public FakeCustomDataSourceReference(Type type) 
            : base(type)
        {
            this.SupportedDataSources = new HashSet<IDataSource>();
        }

        public FakeCustomDataSourceReference(FakeCustomDataSourceReference other) 
            : base(other)
        {
            this.GuidSetter = other.GuidSetter;
            this.NameSetter = other.NameSetter;
            this.DescriptionSetter = other.DescriptionSetter;
            this.DataSourcesSetter = other.DataSourcesSetter;
            this.AvailableTablesSetter = other.AvailableTablesSetter;
            this.CommandLineOptionsSetter = other.CommandLineOptionsSetter;
            this.SupportedDataSources = other.SupportedDataSources;
        }

        public Guid GuidSetter { get; set; }
        public override Guid Guid => this.GuidSetter;

        public string NameSetter { get; set; }
        public override string Name => this.NameSetter;

        public string DescriptionSetter { get; set; }
        public override string Description => this.DescriptionSetter;

        public IReadOnlyCollection<DataSourceAttribute> DataSourcesSetter { get; set; }
        public override IReadOnlyCollection<DataSourceAttribute> DataSources => this.DataSourcesSetter;

        public IEnumerable<TableDescriptor> AvailableTablesSetter { get; set; }
        public override IEnumerable<TableDescriptor> AvailableTables => this.AvailableTablesSetter;

        public IEnumerable<Option> CommandLineOptionsSetter { get; set; }
        public override IEnumerable<Option> CommandLineOptions => this.CommandLineOptionsSetter;

        public HashSet<IDataSource> SupportedDataSources { get; }
        public override bool Supports(IDataSource dataSource)
        {
            return this.SupportedDataSources.Contains(dataSource);
        }

        public override CustomDataSourceReference CloneT()
        {
            return new FakeCustomDataSourceReference(this);
        }

        public override bool Equals(CustomDataSourceReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var fcds = other as FakeCustomDataSourceReference;
            return fcds != null &&
                this.Name == fcds.Name;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FakeCustomDataSourceReference);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
