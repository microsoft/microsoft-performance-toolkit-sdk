// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="IProjection{TSource, TResult}"/> instances in
    ///     regards to interaction with the caller's visible domain.
    /// </summary>
    public static class VisibleDomainSensitiveProjection
    {
        /// <summary>
        ///     Determines if the <see cref="IProjection{TSource, TResult}"/>
        ///     depends on a visible domain.
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
        ///     depends on a visible domain; <c>false</c> otherwise.
        /// </returns>
        public static bool DependsOnVisibleDomain<TSource, TResult>(
            this IProjection<TSource, TResult> self)
        {
            var casted = self as IVisibleDomainSensitiveProjection;
            if (casted != null)
            {
                return casted.DependsOnVisibleDomain;
            }

            return false;
        }

        /// <summary>
        ///     Determines if the <see cref="IProjection{TSource, TResult}"/>
        ///     depends on a visible domain in the application.
        /// </summary>
        /// <typeparam name="TProjection">
        ///     The <see cref="System.Type"/> being interrogated.
        /// </typeparam>
        /// <param name="self">
        ///     The projection being queried.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="IProjection{TSource, TResult}"/>
        ///     depends on the current visible domain; <c>false</c> otherwise.
        /// </returns>
        public static bool DependsOnVisibleDomain<TProjection>(this TProjection self)
            where TProjection : IProjectionDescription
        {
            var casted = self as IVisibleDomainSensitiveProjection;
            if (casted != null)
            {
                return casted.DependsOnVisibleDomain;
            }

            return false;
        }

        /// <summary>
        ///     Clones the given <see cref="IProjection{TSource, TResult}"/>
        ///     if the projection is sensitive to a visible domain; otherwise, the 
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
        public static IProjection<TSource, TResult> CloneIfVisibleDomainSensitive<TSource, TResult>(
            this IProjection<TSource, TResult> self)
        {
            if (self.DependsOnVisibleDomain())
            {
                return (IProjection<TSource, TResult>)((IVisibleDomainSensitiveProjection)self).Clone();
            }

            return self;
        }

        /// <summary>
        ///     Clones the given <typeparamref name="TProjection"/>
        ///     if the projection is sensitive to a visible domain; otherwise, the 
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
        public static TProjection CloneIfVisibleDomainSensitive<TProjection>(this TProjection self)
            where TProjection : IProjectionDescription
        {
            var casted = self as IVisibleDomainSensitiveProjection;
            if (casted != null)
            {
                return (TProjection)casted.Clone();
            }

            return self;
        }

        /// <summary>
        ///     Notifies the given projection of a visible domain change if the
        ///     projection depends on a visible domain.
        /// </summary>
        /// <typeparam name="TProjection">
        ///     The <see cref="System.Type"/> being interrogated.
        /// </typeparam>
        /// <param name="self">
        ///     The projection being notified.
        /// </param>
        /// <param name="newVisibleDomain">
        ///     The new visible domain.
        /// </param>
        public static bool NotifyVisibleDomainChanged<TProjection>(
            this TProjection self,
            IVisibleDomainRegion newVisibleDomain)
            where TProjection : IProjectionDescription
        {
            var casted = self as IVisibleDomainSensitiveProjection;
            if (casted != null)
            {
                return casted.NotifyVisibleDomainChanged(newVisibleDomain);
            }

            return false;
        }
    }
}
