﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Options;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    internal interface IMetadataGen
    {
        int Run(MetadataGenOptions options);
    }
}
