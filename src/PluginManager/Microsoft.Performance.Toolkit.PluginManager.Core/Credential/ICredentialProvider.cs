// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Credential
{
    public interface ICredentialProvider<TSource> where TSource : class, IPluginSource
    {
        Task<ICredentials> GetAsync(TSource source, CancellationToken cancellationToken);
    }
}
