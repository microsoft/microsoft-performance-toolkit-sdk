using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing.ExtensibleColumn.Sample
{
    public class StackTagColumnExtender
        : IColumnExtender<string, string, StackColumnExtensionProperties>
    {
        public static ColumnExtensionDescriptor _ColumnExtensionDescriptor = new ColumnExtensionDescriptor(
            "Stack Tag",
            "Stack Tag Description");

        public ColumnExtensionDescriptor ColumnExtensionDescriptor => _ColumnExtensionDescriptor;

        public IProjection<string> CreateProjection(IProjection<string> sourceProjection, StackColumnExtensionProperties properties)
        {
            bool fold = properties.IsFrameTagFold;

            var projection = (IProjection<string>)Projection.CreateUsingFuncAdaptor<int, string>(i => i.ToString());

            return projection;
        }

        public string CreateColumnName(StackColumnExtensionProperties properties)
        {
            bool fold = properties.IsFrameTagFold;
            string name = "Stack Tag";
            if (fold)
            {
                name += " (Fold)";
            }

            return name;
        }
    }
}
