// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.PluginManager.Core.Credential;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     Provides a base class for implementing <see cref="IPluginDiscoverer"/.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The <see cref="Type"/> of the <see cref="IPluginSource"/> this discover discovers plugins from.
    /// </typeparam>
    public abstract class PluginDiscoverer<TSource> : IPluginDiscoverer<TSource>
        where TSource : class, IPluginSource
    {
        /// <inheritdoc />
        public Type PluginSourceType
        {
            get
            {
                return typeof(TSource);
            }
        }

        public abstract IDiscovererEndpoint CreateDiscovererEndpoint(TSource source, ICrendentialProvider crendentialProvider);

        /// <inheritdoc />
        public abstract bool IsSourceSupported(TSource source);
    }
}
