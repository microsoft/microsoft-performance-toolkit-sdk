using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    public interface IDataColumnExtensible<T, TExtenderType>
        : IDataColumn<T>,
          IReadOnlyCollection<TExtenderType> where TExtenderType : IColumnExtender<T>
    {
    }
}
