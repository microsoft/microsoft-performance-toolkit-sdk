// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    // _CDS_
    // todo: this is not thread safe, does it need to be? I think we're fine without making it thread safe, but not 100% sure.

    /// <summary>
    ///     A helper class for streaming data cookers.
    /// </summary>
    /// <typeparam name="T">Type of data element produced by the data cooker.</typeparam>
    public class StreamingData<T>
    {
        private readonly HashSet<Action<T>> callbacks = new HashSet<Action<T>>();

        /// <summary>
        ///     The number of current subscribers.
        /// </summary>
        /// <remarks>
        ///     This can be useful to check to avoid processing data that won't be consumed.
        /// </remarks>
        public int SubscriberCount => this.callbacks.Count;

        /// <summary>
        ///     Subscribe to the data cooker to receive callbacks for each data element produced.
        /// </summary>
        /// <param name="dataElementCallback">Callback to receive data elements.</param>
        public void Subscribe(Action<T> dataElementCallback)
        {
            this.callbacks.Add(dataElementCallback);
        }

        /// <summary>
        ///     Unsubscribe to the data cooker to stop receiving callbacks for each data element produced.
        /// </summary>
        /// <param name="dataElementCallback">Previously subscribed callback.</param>
        public void Unsubscribe(Action<T> dataElementCallback)
        {
            this.callbacks.Remove(dataElementCallback);
        }

        /// <summary>
        ///     Called to distribute the data element to any listeners.
        /// </summary>
        /// <param name="element">Data element</param>
        public void AddElement(T element)
        {
            foreach (var callback in this.callbacks)
            {
                callback(element);
            }
        }
    }
}
