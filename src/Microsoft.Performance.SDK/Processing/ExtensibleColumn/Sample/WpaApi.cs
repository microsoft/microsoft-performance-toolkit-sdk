using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing.ExtensibleColumn.Sample
{
    // Define in WPA
    public interface IWPAStackColumnExtensionProperties
        : IColumnExtensionProperties
    {
        bool IsFrameTagFold { get; set; }
    }
}
