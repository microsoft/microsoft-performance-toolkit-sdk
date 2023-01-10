// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.PluginManager.Core.Credential;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     A generic interface reprenting a provider that creates <see cref="IPluginDiscoverer"/> that discoveres source of type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The <see cref="Type"/> of the <see cref="IPluginSource"/> this discover discovers plugins from.
    /// </typeparam>
    public interface IPluginDiscovererProvider<TSource> : IPluginDiscovererProvider
        where TSource : class, IPluginSource
    {
        /// <summary>
        ///     Checks the given <paramref name="source"/> is supported by this discoverer.
        /// </summary>
        /// <param name="source">
        ///     The source this discover discovers plugins from.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="source"/> is supported by this discoverer. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool IsSourceSupported(TSource source);

        /// <summary>
        ///     Creates a discoverer for the specified plugin source.
        /// </summary>
        /// <param name="source">
        ///     A plugin source.
        /// </param>
        /// <returns>
        ///     A plugin discoverer.
        /// </returns>
        IPluginDiscoverer CreateDiscoverer(TSource source);

        /// <summary>
        ///     Lazily gets a credential provider that provides credentials for the discoverers created from this <see cref="IPluginDiscovererProvider{TSource}"/>.
        /// </summary>
        Lazy<ICredentialProvider<TSource>> CredentialProvider { get; }
    }
}
