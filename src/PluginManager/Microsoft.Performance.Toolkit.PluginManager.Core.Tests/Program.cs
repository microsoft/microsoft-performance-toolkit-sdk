using Microsoft.Performance.Toolkit.PluginManager.Core.Packaging.Metadata;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Tests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var metadata = new PluginMetadata()
            {
                Group = "Linux",
                Id = "LinuxAndroidTools",
                Version = new Version("1.0.0"),
                DisplayName = "Linux Tool",
                Description = "Linux Tool Bundles",
                TargetPlatforms = new[] { Architecture.X64, Architecture.X86 },
                Authors = new[] { "A", "B", "C", "D" },
                SdkVersion = new Version("1.0.0"),
                ProcessingSourceMetadataCollection = new[]
                {
                    new ProcessingSourceMetadata
                    {
                        Name = "LTTng",
                        Guid = Guid.Parse("98608154-6231-4F25-903A-5E440574AB45"),
                        Version = new Version("2.0.0.0"),
                        AvailableTables = new[]
                        {
                            new TableMetadata
                            {
                                Name = "Trace Stats",
                                Description = "Trace Stats",
                                Category = "Unkown",
                            }
                        },
                        SupportedDataSources = new SupportedDataSourceMetadata[]
                        {
                            new SupportedDataSourceMetadata()
                            {
                                DataSourceType = DataSourceType.FileDataSource,
                                Extension = ".ctf",
                                Description = "Processes LTTng CTF data",
                            },
                        },
                    },
                    new ProcessingSourceMetadata
                    {
                        Name = "WaLinuxAgent",
                        Guid = Guid.Parse("a9ac39bc-2d07-4a01-b9b5-13a02611f5f2"),
                        Version = new Version("1.0.0.0"),
                        SupportedDataSources = new SupportedDataSourceMetadata[]
                        {
                            new SupportedDataSourceMetadata
                            {
                                DataSourceType = DataSourceType.FileDataSource,
                                Extension = ".log",
                                Description = "Linux WaLinuxAgent Cloud Provisioning Log"
                            },
                        }
                    }
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters ={
                    new JsonStringEnumConverter(),
                    new StringConverter(),
                }
            };

            string fileName = "d:\\test\\pluginspec.json";
            using FileStream createStream = File.Create(fileName);
            await JsonSerializer.SerializeAsync(createStream, metadata, options);
            await createStream.DisposeAsync();

            Console.WriteLine(File.ReadAllText(fileName));
        }

        internal class StringConverter : JsonConverter<Version>
        {
            public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var stringValue = reader.GetString();
                    return new Version(stringValue);
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
