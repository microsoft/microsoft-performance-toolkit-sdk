// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public static class Any
    {
        public static TableDescriptor TableDescriptor()
        {
            return new TableDescriptor(
                Guid.NewGuid(),
                "Test " + Guid.NewGuid(),
                "This is a test " + Guid.NewGuid(),
                "What category?",
                false,
                TableLayoutStyle.GraphAndTable);
        }

        public static TableDescriptor MetadataTableDescriptor()
        {
            return new TableDescriptor(
                Guid.NewGuid(),
                "Test " + Guid.NewGuid(),
                "This is a test " + Guid.NewGuid(),
                "What category?",
                true,
                TableLayoutStyle.GraphAndTable);
        }

        public static IDataSource DataSource()
        {
            return new FakeDataSource();
        }

        public static ColumnConfiguration ColumnConfiguration()
        {
            return new ColumnConfiguration(
                new ColumnMetadata(
                    Guid.NewGuid(),
                    Guid.NewGuid() + "_Column",
                    "Test Column"));
        }

        public static IProcessorEnvironment ProcessorEnvironment()
        {
            return new FakeProcessorEnvironment();
        }

        private sealed class FakeDataSource
            : IDataSource
        {
            public Uri GetUri()
            {
                return new Uri("C:/test.txt");
            }
        }
    }
}
