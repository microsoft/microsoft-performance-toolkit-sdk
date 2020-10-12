using System;
using System.IO;
using Newtonsoft.Json;

namespace DataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please provide an output file path.");
                return;
            }

            var serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (var fileWriter = File.CreateText(args[0]))
            {
                using (var writer = new JsonTextWriter(fileWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.IndentChar = ' ';

                    writer.WriteStartObject();

                    var exampleData = SampleData.GenerateEvents(SampleData.ExampleData1);

                    writer.WritePropertyName("DataRecordCount");
                    writer.WriteValue(exampleData.Count);

                    writer.WritePropertyName("WallClock");
                    serializer.Serialize(writer, new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc));

                    writer.WritePropertyName("Data");
                    writer.WriteStartArray();

                    foreach (var value in exampleData)
                    {
                        serializer.Serialize(writer, value);
                    }

                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
            }
        }
    }
}
