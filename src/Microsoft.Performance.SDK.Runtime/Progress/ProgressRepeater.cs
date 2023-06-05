// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Runtime.Progress
{
    /// <summary>
    ///     An <see cref="IProgress{T}"/> that wraps several <see cref="IProgress{T}"/> instances to relay
    ///     the same reported value to.
    /// </summary>
    /// <typeparam name="T">The type of progress update value.</typeparam>
    public sealed class ProgressRepeater<T>
        : IProgress<T>
    {
        private readonly List<IProgress<T>> children;

        public ProgressRepeater(params IProgress<T>[] children)
        {
            this.children = children.ToList();
        }

        /// <inheritdoc />
        public void Report(T value)
        {
            foreach (IProgress<T> child in this.children)
            {
                child?.Report(value);
            }
        }
    }
}
