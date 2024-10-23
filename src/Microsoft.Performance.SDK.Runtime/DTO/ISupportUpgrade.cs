// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    internal interface ISupportUpgrade<T>
    {
        T Upgrade(ILogger logger);
    }
}
