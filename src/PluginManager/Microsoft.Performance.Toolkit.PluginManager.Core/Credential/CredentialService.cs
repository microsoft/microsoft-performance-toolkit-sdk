// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Credential
{
    public class CredentialService : ICredentialService
    {
        private readonly IEnumerable<ICredentialProvider> providers;

        private static readonly Semaphore ProviderSemaphore = new Semaphore(1, 1);

        public CredentialService(IEnumerable<ICredentialProvider> providers)
        {
            this.providers = providers;
        }

        public async Task<ICredentials> GetCredentialsAsync(Uri uri, CancellationToken cancellationToken)
        {
            Guard.NotNull(uri, nameof(uri));

            ICredentials creds = null;

            foreach (ICredentialProvider provider in this.providers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {

                    ProviderSemaphore.WaitOne();

                    creds = await provider.GetAsync(uri, cancellationToken);
                    if (creds != null)
                    {
                        break;
                    }
                }
                finally
                {
                    ProviderSemaphore.Release();
                }
            }

            return creds;
        }
    }
}
