// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Credential
{
    public interface ICrendentialProvider
    {
        Task<ICredentials> GetAsync(Uri uri);
    }
}
