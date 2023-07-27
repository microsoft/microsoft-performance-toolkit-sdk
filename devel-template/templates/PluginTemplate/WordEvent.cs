// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;

namespace SimplePlugin
{
    public class WordEvent
        : IKeyedDataType<string>
    {
        public string Word { get; }
        
        public Timestamp Time { get; }

        public string FilePath { get; }

        public WordEvent(string word, Timestamp timestamp, string filePath)
        {
            this.Word = word;
            this.Time = timestamp;
            this.FilePath = filePath;
        }

        public string GetKey()
        {
            return this.Word; // TODO: update key if needed.
        }
    }
}
