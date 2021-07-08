// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK.Extensibility;
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

        public static string FilePath()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        public static DataCookerPath DataCookerPath()
        {
            return new DataCookerPath();
        }

        public static string FileOnDisk(string extension)
        {
            return FileOnDisk(extension, Path.GetTempPath());
        }

        public static string FileOnDisk(string extension, string folder)
        {
            var fileName = "MICROSOFT-PERFORMANCE-TESTING-SDK-" + Path.GetRandomFileName();
            var file = Path.Combine(folder, fileName) + extension;
            File.WriteAllText(file, "THIS IS A TEST FILE");
            return file;
        }

        public static CustomDataSourceAttribute CustomDataSourceAttribute()
        {
            return new CustomDataSourceAttribute(
                Guid.NewGuid().ToString(),
                "AnyCds",
                "Irrelevant");
        }

        private sealed class FakeDataSource
            : IDataSource
        {
            public Uri Uri => new Uri("C:/test.txt");
        }
    }
}
