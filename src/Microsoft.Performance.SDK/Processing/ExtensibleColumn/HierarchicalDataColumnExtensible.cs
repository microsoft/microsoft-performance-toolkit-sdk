using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    public class HierarchicalDataColumnExtensible<T>
        : HierarchicalDataColumn<T>,
          IDataColumnExtensible<T, IHierachicalColumnExtender<T>>
    {
        private readonly HashSet<IHierachicalColumnExtender<T>> columnExtenders;

        public HierarchicalDataColumnExtensible(
            ColumnConfigurationWithExtensions configuration,
            IProjection<int, T> projection,
            ICollectionInfoProvider<T> collectionProvider,
            IEnumerable<IHierachicalColumnExtender<T>> columnExtenders = null)
            : base(configuration, projection, collectionProvider)
        {
            this.columnExtenders = new HashSet<IHierachicalColumnExtender<T>>(columnExtenders);
        }

        public IReadOnlyCollection<IHierachicalColumnExtender<T>> ColumnExtenders => this.columnExtenders;

        internal void AddColumnExtender(IHierachicalColumnExtender<T> columnExtender)
        {
            this.columnExtenders.Add(columnExtender);
        }

        public IEnumerator<IHierachicalColumnExtender<T>> GetEnumerator()
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
