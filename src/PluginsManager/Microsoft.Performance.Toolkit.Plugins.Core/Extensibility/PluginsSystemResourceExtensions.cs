// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Extensibility
{
    /// <summary>
    ///     Extension methods for <see cref="IPluginsSystemResource"/>.
    /// </summary>
    public static class PluginsSystemResourceExtensions
    {
        /// <summary>
        ///     Extension method to return the <see cref="Guid"/> for a <see cref="IPluginsSystemResource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the <see cref="Guid"/> of the <see cref="IPluginsSystemResource"/>; <see cref="Guid.Empty"/> if not defined.
        /// </returns>
        public static Guid TryGetGuid(this IPluginsSystemResource self)
        {
            Guid? resourceId = self.GetType().GetCustomAttribute<PluginsSystemResourceAttribute>()?.Guid;
            return resourceId.HasValue ? resourceId.Value : Guid.Empty;
        }
    }
}
