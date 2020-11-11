using System.Collections;
using System.Collections.Generic;
using DataExtensionsSample.DataTypes;
using Microsoft.Performance.SDK;
using DataExtensionsSample;
using SampleExtensions.DataTypes;

namespace DataGenerator
{
    public static class SampleData
    {
        public static List<SampleEvent> GenerateEvents(ArrayList data)
        {
            List<SampleEvent> output = new List<SampleEvent>();

            var currentTime = new Timestamp(34);

            foreach (var value in data)
            {
                currentTime += new TimestampDelta(23487);
                
                var dataRecord = new SampleEvent
                {
                    Timestamp = new Timestamp(currentTime.ToNanoseconds),
                    Name = KeyMappings.MappingFromType[value.GetType()],
                    Value = JsonBinary.ToBinary(value)
                };

                output.Add(dataRecord);
            }

            return output;
        }

        public static ArrayList ExampleData1 = new ArrayList()
        {
            new ProcessLoad {Name="wpa.exe", Id =1},
            new ProcessLoad{Name = "mail.exe", Id=3},
            new ImageLoad {Path = @"c:\wpa\wpa.exe", ProcessId=1, LoadAddress=0x80000000, ImageSize=63425},
            new ImageLoad {Path = @"c:\wpa\microsoft.performance.sdk.dll", ProcessId=1, LoadAddress=0x80060000, ImageSize=98135},
            new ImageLoad {Path = @"c:\wpa\microsoft.performance.ui.dll", ProcessId=1, LoadAddress=0x80700000, ImageSize=49813},
            new ImageLoad {Path = @"c:\wpa\microsoft.performance.shell.dll", ProcessId=1, LoadAddress=0x80770000, ImageSize=613187},
            new ImageLoad {Path = @"c:\wpa\microsoft.performance.base.dll", ProcessId=1, LoadAddress=0x80810000, ImageSize=63425},
            new ImageLoad{Path=@"c:\mail\mail.exe", ProcessId=3, LoadAddress=0x80000000, ImageSize=6341681},
            new ImageLoad{Path = @"c:\mail\mail_helper.dll", ProcessId=3, LoadAddress=0x80740000, ImageSize=648916},
            new ImageLoad{Path = @"c:\windows\system32\kernel32.dll", ProcessId=3, LoadAddress=0x80880000, ImageSize=984164},
            new CSwitchData{NewThreadId = 50, OldThreadId = 13},
            new CSwitchData{NewThreadId = 68, OldThreadId = 50}
        };
    }
}
