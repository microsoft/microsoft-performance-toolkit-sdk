// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime.Progress
{
    /// <summary>
    ///     Responsible for aggregating the progress of different processes to a single <see cref="IProgress{T}"/>.
    /// </summary>
    /// <typeparam name="TAggregate">
    ///     The type of the aggregated value to report to a single <see cref="IProgress{T}"/>.
    /// </typeparam>
    /// <typeparam name="T">
    ///     The type of the individual <see cref="IProgress{T}"/> values that will be aggregated into <typeparamref name="TAggregate"/>.
    /// </typeparam>
    public class ProgressAggregator<TAggregate, T>
    {
        private readonly Func<IEnumerable<T>, TAggregate> aggregateDelegate;
        private readonly IProgress<TAggregate> aggregateTo;

        private readonly ConcurrentDictionary<Progress<T>, T>
            currentValues = new ConcurrentDictionary<Progress<T>, T>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressAggregator{TAggregate,T}"/> class.
        /// </summary>
        /// <param name="aggregateTo">
        ///     The <see cref="IProgress{T}"/> that should receive the aggregated progress value whenever new progress
        ///     by a child is reported.
        /// </param>
        /// <param name="aggregateDelegate">
        ///     A delegate for performing the aggregation from multiple <typeparamref name="T"/> to a single <typeparamref name="TAggregate"/>.
        /// </param>
        public ProgressAggregator(
            IProgress<TAggregate> aggregateTo,
            Func<IEnumerable<T>, TAggregate> aggregateDelegate)
        {
            this.aggregateTo = aggregateTo;
            this.aggregateDelegate = aggregateDelegate;
        }

        /// <summary>
        ///     Adds a new child <see cref="IProgress{T}"/> to be aggregated.
        /// </summary>
        /// <param name="initialValue">
        ///     The initial value the created progress should report.
        /// </param>
        /// <returns>
        ///     The created child <see cref="IProgress{T}"/>.
        /// </returns>
        public IProgress<T> CreateChild(T initialValue)
        {
            var child = new Progress<T>();
            child.ProgressChanged += OnProgressChanged;

            this.currentValues.TryAdd(child, initialValue);
            UpdateAggregatedValue();

            return child;
        }

        /// <summary>
        ///     Denotes that the process being tracked by the aggregated progress has finished.
        /// </summary>
        /// <param name="finalValue">
        ///     The final value to report.
        /// </param>
        public void Finish(TAggregate finalValue)
        {
            foreach (var child in this.currentValues.Keys)
            {
                child.ProgressChanged -= OnProgressChanged;
            }

            this.currentValues.Clear();
            this.aggregateTo?.Report(finalValue);
        }

        private void OnProgressChanged(object sender, T e)
        {
            if (!(sender is Progress<T> progress)) return;

            this.currentValues[progress] = e;
            UpdateAggregatedValue();
        }

        private void UpdateAggregatedValue()
        {
            // This should be impossible, since we only call this method after a child is created
            Debug.Assert(currentValues.Count > 0);

            this.aggregateTo?.Report(this.aggregateDelegate(this.currentValues.Values));
        }
    }
}