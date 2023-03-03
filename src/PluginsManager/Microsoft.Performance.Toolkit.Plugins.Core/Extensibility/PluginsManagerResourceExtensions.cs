// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Reflection;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Extensibility
{
    /// <summary>
    ///     Extension methods for <see cref="IPluginsManagerResource"/>.
    /// </summary>
    public static class PluginsManagerResourceExtensions
    {
        /// <summary>
        ///     Extension method to return the <see cref="Guid"/> for a <see cref="IPluginsManagerResource"/>.
        /// </summary>
        /// <returns>
        ///     Returns the <see cref="Guid"/> of the <see cref="IPluginsManagerResource"/>; <see cref="Guid.Empty"/> if not defined.
        /// </returns>
        public static Guid TryGetGuid(this IPluginsManagerResource self)
        {
            Guid? resourceId = self.GetType().GetCustomAttribute<PluginsManagerResourceAttribute>()?.Guid;
            return resourceId.HasValue ? resourceId.Value : Guid.Empty;
        }
    }
}
