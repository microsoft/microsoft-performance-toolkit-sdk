using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    public sealed class ColumnExtensionDescriptor
    {
        public string Name { get; }

        public string Description { get; }


        public ColumnExtensionDescriptor(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
