// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace Microsoft.Performance.SDK.Runtime.DTO;

[DataContract]
internal abstract class PreviousPrebuiltConfigurationBase<TNext>
    : PrebuiltConfigurationsBase,
      ISupportUpgrade<PrebuiltConfigurationsBase>
    where TNext : PrebuiltConfigurationsBase
{
    public PrebuiltConfigurationsBase Upgrade()
    {
        return UpgradeToNext();
    }

    protected abstract TNext UpgradeToNext();
}