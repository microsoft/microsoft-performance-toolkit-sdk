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
        ///     <see cref="PluginsLoader"/> will call this method for every _unique_ processing
        ///     source loaded by plugins.
        ///     <para/>
        ///     The <see cref="PluginsLoader"/> will block and not allow future plugins to be
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
        /// <param name="processingSource">
        ///     The custom data source loaded.
        /// </param>
        void OnProcessingSourceLoaded(string pluginName, Version pluginVersion, ProcessingSourceReference processingSource);
    }
}