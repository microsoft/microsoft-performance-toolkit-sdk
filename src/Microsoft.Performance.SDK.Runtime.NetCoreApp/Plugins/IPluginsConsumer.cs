// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Plugins
{
    public interface IPluginsConsumer
    {
        /// <summary>
        ///     Process a custom data source provided by a loaded plugin.
        ///     <para/>
        ///     <see cref="PluginsLoader"/> will call this method for every _unique_ custom data
        ///     source loaded by plugins.
        ///     <para/>
        ///     The <see cref="PluginLoader"/> will block and not allow future plugins to be
        ///     loaded until this method returns, so it should not block or attempt to load additional plugins.
        /// </summary>
        /// <param name="pluginName">
        ///     The name of the plugin which provided the custom data source, or <c>null</c> if one could
        ///     not be determined.
        /// </param>
        /// <param name="pluginVersion">
        ///     The version of the plugin which provided the custom data source, or <c>null</c> if one could
        ///     not be determined.
        /// </param>
        /// <param name="customDataSource">
        ///     The custom data source loaded.
        /// </param>
        void OnCustomDataSourceLoaded(string pluginName, Version pluginVersion, CustomDataSourceReference customDataSource);
    }
}