using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    // To be extended by interfaces in WPA SDK
    public enum StackDisplayModes
    {
        Frames,
        FrameTags,
        StackTag,
        TagHierarchy,
    };

    public class ColumnExtensionConguration
        : ICloneable<ColumnExtensionConguration>
    {
        public ColumnExtensionConguration(ColumnExtensionDescriptor descriptor, IColumnExtensionProperties properties)
        {
            this.ColumnExtensionDescriptor = descriptor;
            this.ColumnExtensionProperties = properties;
        }

        ColumnExtensionDescriptor ColumnExtensionDescriptor { get; }

        IColumnExtensionProperties ColumnExtensionProperties { get; }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public ColumnExtensionConguration CloneT()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Example of using this interface in a plugin
    /// </summary>
    /// 

    public class Test
    {
        public void TestConfig()
        {
            var cec = new ColumnExtensionConguration(StackTagColumnExtender._ColumnExtensionDescriptor, new StackColumnExtensionProperties());
            var ccwe  = new ColumnConfigurationWithExtensions(null, null, cec);


        }
    }
}
