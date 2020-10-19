// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// Exposes a function that has one parameter of the type specified by the <typeparamref name="TSource"/> parameter and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TSource">The type of the function parameter.</typeparam>
    /// <typeparam name="TResult">The type of the function result.</typeparam>
    public interface IProjector<in TSource, out TResult>
    {
        /// <summary>
        /// Get the element for the specified value.
        /// </summary>
        /// <param name="value">The function parameter.</param>
        /// <returns></returns>
        TResult this[TSource value] { get; }
    }
}
