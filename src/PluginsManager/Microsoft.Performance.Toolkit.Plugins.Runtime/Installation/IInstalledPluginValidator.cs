// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///    Validates an installed plugin.
    /// </summary>
    public interface IInstalledPluginValidator
    {
        /// <summary>
        ///     Validates the given installed plugin.
        /// </summary>
        /// <param name="installedPlugin">
        ///     The installed plugin to validate.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the installed plugin is valid, <c>false</c> otherwise.
        /// </returns>
        Task<bool> ValidateInstalledPluginAsync(InstalledPluginInfo installedPlugin);
    }
}
