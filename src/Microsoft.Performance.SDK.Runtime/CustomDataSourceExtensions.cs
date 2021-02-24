// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Extension methods for <see cref="ICustomDataSource"/>.
    /// </summary>
    public static class CustomDataSourceExtensions
    {
        /// <summary>
        ///     Extension method to return the name for a <see cref="ICustomDataSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the name of the <see cref="ICustomDataSource"/>; <c>null</c> if not defined.
        /// </returns>
        public static string TryGetName(this ICustomDataSource self)
        {
            return self.GetType().GetCustomAttribute<CustomDataSourceAttribute>()?.Name;
        }

        /// <summary>
        ///     Extension method to return the description for a <see cref="ICustomDataSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the description of the <see cref="ICustomDataSource"/>; <c>null</c> if not defined.
        /// </returns>
        public static string TryGetDescription(this ICustomDataSource self)
        {
            return self.GetType().GetCustomAttribute<CustomDataSourceAttribute>()?.Description;
        }

        /// <summary>
        ///     Extension method to return the <see cref="Guid"/> for a <see cref="ICustomDataSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the <see cref="Guid"/> of the <see cref="ICustomDataSource"/>; <see cref="Guid.Empty"/> if not defined.
        /// </returns>
        public static Guid TryGetGuid(this ICustomDataSource self)
        {
            var dataSourceId = self.GetType().GetCustomAttribute<CustomDataSourceAttribute>()?.Guid;
            return dataSourceId.HasValue ? dataSourceId.Value : Guid.Empty;
        }
    }
}
