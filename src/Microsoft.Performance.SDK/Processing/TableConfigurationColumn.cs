using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    public class TableConfigurationColumn
    {
        public TableConfigurationColumn(ColumnIdentifier columnIdentifier)
            : this(columnIdentifier, null)
        {
        }

        public TableConfigurationColumn(ColumnIdentifier columnIdentifier, UIHints displayHints)
        {
            ColumnIdentifier = columnIdentifier;
            this.DisplayHints = displayHints;
        }

        public ColumnIdentifier ColumnIdentifier { get; }

        public UIHints DisplayHints { get; }
    }
}
