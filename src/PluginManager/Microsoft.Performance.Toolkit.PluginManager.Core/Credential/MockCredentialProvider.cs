// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Credential
{
    public class MockCredentialProvider : ICrendentialProvider
    {
        public Task<ICredentials> GetAsync(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
