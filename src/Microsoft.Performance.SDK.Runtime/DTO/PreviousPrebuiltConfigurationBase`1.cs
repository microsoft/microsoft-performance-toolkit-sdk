// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
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
        var next = UpgradeToNext();
        if (next.Version <= this.Version)
        {
            throw new InvalidOperationException(
                $"Cannot upgrade to a version less than or equal to the current version. Current version: {this.Version}, Next version: {next.Version}");
        }

        return next;
    }

    /// <summary>
    ///     Upgrade the current configuration to the next version. The version number
    ///     of the returned configuration MUST be greater than this instance's version number.
    /// </summary>
    /// <returns>
    ///     The upgraded configuration.
    /// </returns>
    protected abstract TNext UpgradeToNext();
}