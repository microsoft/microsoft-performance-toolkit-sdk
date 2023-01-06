// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.NuGet
{
    public sealed class NuGetPluginSource : UriPluginSource
    {
        public NuGetPluginSource(string name, Uri uri) : base(name, uri)
        {
        }
    }
}
