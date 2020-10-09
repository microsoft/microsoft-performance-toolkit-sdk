// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// Exposes a function that has one parameter of the type specified by the <typeparamref name="TSource"/> parameter and returns a value of the type specified by the <typeparam name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TSource">The type of the function parameter.</typeparam>
    public interface IProjection<in TSource, out TResult>
        : IProjectionDescription,
          IProjector<TSource, TResult>
    {
    }

    /// <summary>
    /// Exposes a function that has an input parameter of type 'int' and returns a value of the type specified by the <typeparam name="TResult"/> parameter.
    /// </summary>
    public interface IProjection<out TResult> 
        : IProjection<int, TResult>
    {
    }
}
