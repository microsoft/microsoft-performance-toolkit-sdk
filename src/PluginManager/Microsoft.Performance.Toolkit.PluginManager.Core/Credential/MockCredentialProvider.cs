// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Credential
{
    public class MockCredentialProvider : ICredentialProvider
    {
        public Task<ICredentials> GetAsync(Uri uri, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
