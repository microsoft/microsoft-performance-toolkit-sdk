// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// A helper class for building up data without having to reallocate.
    /// </summary>
    /// <typeparam name="T">Data type to store</typeparam>
    public class ProcessedEventData<T>
        : IReadOnlyList<T>
    {
        private const int OuterIndexShift = 16;
        private const uint BuilderListSize = 1 << OuterIndexShift;
        private const uint InnerIndexMask = BuilderListSize - 1;

        private LinkedList<List<T>> eventsBuilder = new LinkedList<List<T>>();
        private List<T> currentEventsBuilderList;

        private List<List<T>> events;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ProcessedEventData()
        {
            this.CreateNewList();
        }

        /// <summary>
        /// The number of data elements added.
        /// </summary>
        public uint Count { get; private set; }

        int IReadOnlyCollection<T>.Count => (int)this.Count;

        public T this[int index] => this[(uint)index];

        /// <summary>
        /// Add a data element.
        /// </summary>
        /// <param name="newEvent">Data element to add.</param>
        public void AddEvent(T newEvent)
        {
            if (this.eventsBuilder == null)
            {
                throw new InvalidOperationException("Objects has been finalized. No more data may be added.");
            }

            if (this.ListIsFull())
            {
                this.CreateNewList();
            }

            this.currentEventsBuilderList.Add(newEvent);
            this.Count++;
        }

        /// <summary>
        /// Remove all added events.
        /// </summary>
        public void Clear()
        {
            this.events.Clear();
        }

        /// <summary>
        /// Prepare data for indexing once all data elements have been added.
        /// </summary>
        public void FinalizeData()
        {
            this.events = new List<List<T>>(this.eventsBuilder.Count);

            foreach (var eventList in this.eventsBuilder)
            {
                this.events.Add(eventList);
            }

            this.eventsBuilder = null;
            this.currentEventsBuilderList = null;
        }

        /// <summary>
        /// Access data elements through indexing.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Data element</returns>
        public T this[uint index]
        {
            get
            {
                Debug.Assert(this.events != null);
                Debug.Assert(index <= this.Count);

                uint outerIndex = index >> OuterIndexShift;
                uint innerIndex = index & InnerIndexMask;

                Debug.Assert(outerIndex < this.events.Count);
                Debug.Assert(innerIndex < this.events[(int)outerIndex].Count);

                return this.events[(int)outerIndex][(int)innerIndex];
            }
        }

        private void CreateNewList()
        {
            this.currentEventsBuilderList = new List<T>((int)BuilderListSize);
            this.eventsBuilder.AddLast(this.currentEventsBuilderList);
        }

        private bool ListIsFull()
        {
            return this.currentEventsBuilderList.Count == BuilderListSize;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for(uint i = 0; i < this.Count; ++i)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
