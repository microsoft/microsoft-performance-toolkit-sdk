// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Extensibility
{
    /// <summary>
    ///     Extension methods for <see cref="IPluginManagerResource"/>.
    /// </summary>
    public static class PluginManagerResourceExtensions
    {
        /// <summary>
        ///     Extension method to return the <see cref="Guid"/> for a <see cref="IPluginManagerResource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the <see cref="Guid"/> of the <see cref="IPluginManagerResource"/>; <see cref="Guid.Empty"/> if not defined.
        /// </returns>
        public static Guid TryGetGuid(this IPluginManagerResource self)
        {
            Guid? resourceId = self.GetType().GetCustomAttribute<PluginManagerResourceAttribute>()?.Guid;
            return resourceId.HasValue ? resourceId.Value : Guid.Empty;
        }
    }
}
