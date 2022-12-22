using Microsoft.Performance.SDK.Processing;
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
                Id = "LinuxAndroidTools",
                Version = new Version("1.0.0"),
                DisplayName = "Linux Tool",
                Description = "Linux Tool Bundles",
                Owners = new[]
                {
                    new PluginOwner()
                    {
                        Name = "Someone",
                        EmailAddresses = new[] {"abc@outlook.com", "xyz@outlook.com" },
                        PhoneNumbers = new [] {"123-333-4566"}
                    }
                },
                SdkVersion = new Version("1.0.0"),
                ProcessingSources = new[]
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
                                Guid = Guid.Parse("F8F953B2-DFED-448B-87DD-A15CB6DD1D6D"),
                                Name = "Trace Stats",
                                Description = "Trace Stats",
                                Category = "Unkown",
                                IsMetadataTable = false,
                            }
                        },
                        AboutInfo = new SDK.Processing.ProcessingSourceInfo
                        {
                            Owners = new[]
                            {
                                new ContactInfo()
                                {
                                    Name = "ps1",
                                    EmailAddresses = new[] {"processingsource1@micrsoft.com"},
                                    PhoneNumbers = new[] {"123-456-7890"}
                                },
                            },
                            ProjectInfo = new ProjectInfo()
                            {
                                Uri = "github.com"
                            },
                            LicenseInfo = new LicenseInfo()
                            {
                                Name = "MIT",
                                Text = "MIT",
                                Uri = "license.com",
                            },
                            CopyrightNotice = "Copyright notice", 
                        },
                        SupportedDataSources = new DataSourceMetadata[]
                        {
                            new DataSourceMetadata()
                            {
                                Name = ".ctf",
                                Description = "Processes LTTng CTF data",
                            },
                        },
                    },
                    new ProcessingSourceMetadata
                    {
                        Name = "WaLinuxAgent",
                        Guid = Guid.Parse("a9ac39bc-2d07-4a01-b9b5-13a02611f5f2"),
                        Version = new Version("1.0.0.0"),
                        SupportedDataSources = new DataSourceMetadata[]
                        {
                            new DataSourceMetadata
                            {
                                Name = ".log",
                                Description = "Linux WaLinuxAgent Cloud Provisioning Log"
                            },
                        }
                    }
                },
                DataCookers = new[]
                {
                    new DataCookerMetadata()
                    {
                        Name = "dc1",
                        Description = "dc1",
                    }
                },
                ExtensibleTables = new[]
                {
                     new TableMetadata
                    {
                        Guid = Guid.Parse("6E8AD993-CEF3-40C2-826A-078E21694C8F"),
                        Name = "Extensible Table",
                        Description = "extensible table",
                        Category = "Unkown",
                        IsMetadataTable = false,
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
                    string stringValue = reader.GetString();
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
