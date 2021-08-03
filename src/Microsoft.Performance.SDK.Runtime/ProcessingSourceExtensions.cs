// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Extension methods for <see cref="IProcessingSource"/>.
    /// </summary>
    public static class ProcessingSourceExtensions
    {
        /// <summary>
        ///     Extension method to return the name for a <see cref="IProcessingSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the name of the <see cref="IProcessingSource"/>; <c>null</c> if not defined.
        /// </returns>
        public static string TryGetName(this IProcessingSource self)
        {
            return self.GetType().GetCustomAttribute<ProcessingSourceAttribute>()?.Name;
        }

        /// <summary>
        ///     Extension method to return the description for a <see cref="IProcessingSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the description of the <see cref="IProcessingSource"/>; <c>null</c> if not defined.
        /// </returns>
        public static string TryGetDescription(this IProcessingSource self)
        {
            return self.GetType().GetCustomAttribute<ProcessingSourceAttribute>()?.Description;
        }

        /// <summary>
        ///     Extension method to return the <see cref="Guid"/> for a <see cref="IProcessingSource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the <see cref="Guid"/> of the <see cref="IProcessingSource"/>; <see cref="Guid.Empty"/> if not defined.
        /// </returns>
        public static Guid TryGetGuid(this IProcessingSource self)
        {
            var dataSourceId = self.GetType().GetCustomAttribute<ProcessingSourceAttribute>()?.Guid;
            return dataSourceId.HasValue ? dataSourceId.Value : Guid.Empty;
        }
    }
}
