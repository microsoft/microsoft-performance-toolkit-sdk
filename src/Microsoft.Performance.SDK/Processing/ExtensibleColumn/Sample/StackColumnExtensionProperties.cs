using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing.ExtensibleColumn.Sample
{
    public class StackColumnExtensionProperties
        : IWPAStackColumnExtensionProperties
    {
        public bool IsFrameTagFold { get; set; }
    }

}
