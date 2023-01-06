// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.Toolkit.PluginManager.Core.Credential;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     A discoverer source that creates <see cref="IPluginDiscoverer"/> that discoveres source of type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">
    ///     The <see cref="Type"/> of the <see cref="IPluginSource"/> this discover discovers plugins from.
    /// </typeparam>
    public interface IPluginDiscovererSource<in TSource> : IPluginDiscovererSource
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
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="crendentialProvider"></param>
        /// <returns></returns>
        IPluginDiscoverer CreateDiscoverer(TSource source);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="credentialProviders"></param>
        void SetupCredentialService(IEnumerable<ICredentialProvider> credentialProviders); 
    }
}
