// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="IProjection{TSource, TResult}"/> instances in
    ///     regards to interaction with the caller's viewport.
    /// </summary>
    public static class ViewportSensitiveProjection
    {
        /// <summary>
        ///     Determines if the <see cref="IProjection{TSource, TResult}"/>
        ///     depends on the current viewport in the application.
        /// </summary>
        /// <typeparam name="TSource">
        ///     The <see cref="System.Type"/> of argument to the projection.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The <see cref="System.Type"/> of the result of the projection.
        /// </typeparam>
        /// <param name="self">
        ///     The projection being queried.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="IProjection{TSource, TResult}"/>
        ///     depends on the viewport; <c>false</c> otherwise.
        /// </returns>
        public static bool DependsOnViewport<TSource, TResult>(
            this IProjection<TSource, TResult> self)
        {
            var casted = self as IViewportSensitiveProjection;
            if (casted != null)
            {
                return casted.DependsOnViewport;
            }

            return false;
        }

        /// <summary>
        ///     Determines if the <see cref="IProjection{TSource, TResult}"/>
        ///     depends on the current viewport in the application.
        /// </summary>
        /// <typeparam name="TProjection">
        ///     The <see cref="System.Type"/> being interrogated.
        /// </typeparam>
        /// <param name="self">
        ///     The projection being queried.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="IProjection{TSource, TResult}"/>
        ///     depends on the viewport; <c>false</c> otherwise.
        /// </returns>
        public static bool DependsOnViewport<TProjection>(this TProjection self)
            where TProjection : IProjectionDescription
        {
            var casted = self as IViewportSensitiveProjection;
            if (casted != null)
            {
                return casted.DependsOnViewport;
            }

            return false;
        }

        /// <summary>
        ///     Clones the given <see cref="IProjection{TSource, TResult}"/>
        ///     if the projection is sensitive to the viewport; otherwise, the 
        ///     projection itself is returned unmodified.
        /// </summary>
        /// <typeparam name="TSource">
        ///     The <see cref="System.Type"/> of argument to the projection.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The <see cref="System.Type"/> of the result of the projection.
        /// </typeparam>
        /// <param name="self">
        ///     The projection being cloned.
        /// </param>
        /// <returns>
        ///     A clone of the specified projection, if possible;
        ///     <paramref name="self"/> otherwise.
        /// </returns>
        public static IProjection<TSource, TResult> CloneIfViewportSensitive<TSource, TResult>(
            this IProjection<TSource, TResult> self)
        {
            if (self.DependsOnViewport())
            {
                return (IProjection<TSource, TResult>)((IViewportSensitiveProjection)self).Clone();
            }

            return self;
        }

        /// <summary>
        ///     Clones the given <typeparamref name="TProjection"/>
        ///     if the projection is sensitive to the viewport; otherwise, the 
        ///     projection itself is returned unmodified.
        /// </summary>
        /// <typeparam name="TProjection">
        ///     The <see cref="System.Type"/> being interrogated.
        /// </typeparam>
        /// <param name="self">
        ///     The projection being cloned.
        /// </param>
        /// <returns>
        ///     A clone of the specified projection, if possible;
        ///     <paramref name="self"/> otherwise.
        /// </returns>
        public static TProjection CloneIfViewportSensitive<TProjection>(this TProjection self)
            where TProjection : IProjectionDescription
        {
            var casted = self as IViewportSensitiveProjection;
            if (casted != null)
            {
                return (TProjection)casted.Clone();
            }

            return self;
        }

        /// <summary>
        ///     Notifies the given projection of a viewport change if the
        ///     projection depends on the viewport.
        /// </summary>
        /// <typeparam name="TProjection">
        ///     The <see cref="System.Type"/> being interrogated.
        /// </typeparam>
        /// <param name="self">
        ///     The projection being notified.
        /// </param>
        /// <param name="newViewport">
        ///     The new viewport.
        /// </param>
        public static bool NotifyViewportChanged<TProjection>(
            this TProjection self,
            IVisibleTableRegion newViewport)
            where TProjection : IProjectionDescription
        {
            var casted = self as IViewportSensitiveProjection;
            if (casted != null)
            {
                return casted.NotifyViewportChanged(newViewport);
            }

            return false;
        }
    }
}
