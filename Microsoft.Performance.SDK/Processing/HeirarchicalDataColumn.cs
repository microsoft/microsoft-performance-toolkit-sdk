// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Represents a data column that can display values in a
    ///     hierarchical format. For example, stacks are hierarchical.
    ///     <para/>
    ///     Expanding on the example, a stack is a collection of values
    ///     that should logically be treated as one value. As one expands
    ///     the stack, each call becomes its own row. This column type allows
    ///     for you to provide the means of displaying those expansions in
    ///     a meaningful way.
    /// </summary>
    /// <typeparam name="T">
    ///     The <see cref="Type"/> of data exposed by this column.
    /// </typeparam>
    public class HierarchicalDataColumn<T>
        : BaseDataColumn<T>,
          IHierarchicalDataColumn<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HierarchicalDataColumn{T}"/>
        ///     class.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration of this column.
        /// </param>
        /// <param name="projection">
        ///     The projection that projects the data in the column.
        /// </param>
        /// <param name="collectionProvider">
        ///     The providers that define how to display the hierarchical data.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="configuration"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="projection"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="collectionProvider"/> is <c>null</c>.
        /// </exception>
        public HierarchicalDataColumn(
            ColumnConfiguration configuration,
            IProjection<int, T> projection,
            ICollectionInfoProvider<T> collectionProvider)
            : base(configuration, projection)
        {
            Guard.NotNull(collectionProvider, nameof(collectionProvider));

            Type collectionOutputType = null;

            foreach (var i in collectionProvider.GetType().GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    var genericInterface = i.GetGenericTypeDefinition();
                    if (genericInterface == typeof(ICollectionAccessProvider<,>))
                    {
                        Type collectionInputType = i.GetGenericArguments()[0];
                        collectionOutputType = i.GetGenericArguments()[1];
                        if (collectionInputType != typeof(T))
                        {
                            throw new InvalidOperationException(
                                $"TCollection on the ICollectionAccessProvider<TCollection, TElement>implemented on " + 
                                $"{nameof(collectionProvider)} doesn't match T of {nameof(HierarchicalDataColumn<T>)} from column " +
                                $"{configuration.Metadata.Guid}. TCollection = {collectionInputType.Name}, T = {typeof(T).Name}");
                        }

                        break;
                    }
                }
            }

            if (collectionOutputType == null)
            {
                throw new InvalidOperationException(
                    $"No valid ICollectionAccessProvider<,> found on {nameof(HierarchicalDataColumn<T>)} {configuration.Metadata.Guid}.");
            }

            this.CollectionInfoProvider = collectionProvider;
        }

        /// <inheritdoc />
        public ICollectionInfoProvider<T> CollectionInfoProvider { get; }
    }
}
