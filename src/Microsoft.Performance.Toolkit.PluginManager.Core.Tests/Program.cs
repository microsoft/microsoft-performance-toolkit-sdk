using Microsoft.Performance.Toolkit.PluginManager.Core.Packaging.Metadata;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Tests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var metadata = new PluginMetadata()
            {
                Group = "WPA",
                Id = "LTTng",
                Version = new Version("1.0.0"),
                DisplayName = "Linux Tool",
                Description = "Linux Tool Bundles",
                ProcessingSourceMetadataCollection = new[]
                {
                    new ProcessingSourceMetadata()
                    {
                        SupportedFileDataSources = new SupportedFileDataSource[]
                        {
                            new SupportedFileDataSource()
                            {
                                Extension = "txt",
                                Description = "efsfdsfdf",
                            },
                        },
                        SupportedFolderDataSources = new SupportedFolderDataSource[]
                        {
                            new SupportedFolderDataSource()
                            {
                                Description = "ddsfdfsfdsf"
                            }
                        }

                    }
                }
            };

            var options = new JsonSerializerOptions
            { 
                WriteIndented = true,

            };
            string fileName = "d:\\pluginspec.json";
            using FileStream createStream = File.Create(fileName);
            await JsonSerializer.SerializeAsync(createStream, metadata, options);
            await createStream.DisposeAsync();

            Console.WriteLine(File.ReadAllText(fileName));
        }
    }
}
