// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    internal interface ISupportUpgrade<T>
    {
        T Upgrade();
    }
}
