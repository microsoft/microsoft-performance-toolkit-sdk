using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    public class DataColumnExtensible<T>
        : DataColumn<T>,
          IDataColumnExtensible<T, IColumnExtender<T>>
    {
        private readonly HashSet<IColumnExtender<T>> columnExtenders;

        public DataColumnExtensible(
            ColumnConfigurationWithExtensions columnConfiguration,
            IProjection<int, T> projection,
            IEnumerable<IColumnExtender<T>> columnExtenders = null)
            : base(columnConfiguration, projection)
        {
            this.columnExtenders = new HashSet<IColumnExtender<T>>(columnExtenders);
        }

        public IReadOnlyCollection<IColumnExtender<T>> ColumnExtenders => this.columnExtenders;

        internal void AddColumnExtender(IColumnExtender<T> columnExtender)
        {
            this.columnExtenders.Add(columnExtender);
        }

        public IEnumerator<IColumnExtender<T>> GetEnumerator()
        {
            return this.ColumnExtenders.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count => ColumnExtenders.Count;
    }
}
