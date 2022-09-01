using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Performance.SDK.Processing
{
    public interface IColumnExtender<TSource>
    {
        ColumnExtensionDescriptor ColumnExtensionDescriptor { get; }
    }

    public interface IColumnExtender<TSource, TResult, TConfiguration>
        : IColumnExtender<TSource>
        where TConfiguration : IColumnExtensionProperties
    {
        // Extends the source projection to a new projection
        // Do we want to pass in the source projection here?
        IProjection<TResult> CreateProjection(
            IProjection<TSource> sourceProjection,
            TConfiguration ColumnExtensionConguration);

        string CreateColumnName(TConfiguration ColumnExtensionConguration);
    }

    public interface IHierachicalColumnExtender<TSource>
        : IColumnExtender<TSource>
    {
    }

    public interface IHierachicalColumnExtender<TSource, TResult, TConfiguration>
        : IColumnExtender<TSource, TResult, TConfiguration>,
          IHierachicalColumnExtender<TSource>
         where TConfiguration : IColumnExtensionProperties
    {
        ICollectionInfoProvider<TResult> CreateCollectionInfoProvider(ICollectionInfoProvider<TSource> sourceCollectionAccessProvider);
    }
}
