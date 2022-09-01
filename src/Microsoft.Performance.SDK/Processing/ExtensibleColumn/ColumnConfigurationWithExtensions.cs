using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    public class ColumnConfigurationWithExtensions
        : ColumnConfiguration
    {
        public ColumnConfigurationWithExtensions(ColumnMetadata metadata, UIHints hints, ColumnExtensionConguration extensionConguration)
            : base(metadata, hints)
        {
            this.ExtensionConguration = extensionConguration;
        }

        public ColumnConfigurationWithExtensions(ColumnMetadata metadata, ColumnExtensionConguration extensionConguration) 
            : base(metadata)
        {
            this.ExtensionConguration = extensionConguration;
        }

        public ColumnConfigurationWithExtensions(ColumnConfigurationWithExtensions other) 
            : base(other)
        {
            this.ExtensionConguration = other.ExtensionConguration.CloneT();
        }

        // This configures a column to use a certain extension. Set this to null if you want to use the default config and projection 
        public ColumnExtensionConguration ExtensionConguration { get; }
    }
}
