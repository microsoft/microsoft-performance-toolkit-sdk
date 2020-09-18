// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for
    ///     interacting with the <see cref="TableConfigurations"/>
    ///     class.
    /// </summary>
    public static class TableConfigurationsExtensions
    {
        /// <summary>
        ///     Attempts to get the default configuration from
        ///     the given collection of configurations.
        /// </summary>
        /// <param name="self">
        ///     The instance being operated upon.
        /// </param>
        /// <param name="configuration">
        ///     The default configuration in the collection, if
        ///     the default exists. <c>null</c> otherwise.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the default is retrieved;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool TryGetDefaultConfiguration(
            this TableConfigurations self,
            out TableConfiguration configuration)
        {
            if (self is null)
            {
                configuration = null;
                return false;
            }

            if (self.Configurations is null)
            {
                configuration = null;
                return false;
            }

            var name = self.DefaultConfigurationName;
            configuration = null;
            foreach (var c in self.Configurations)
            {
                if (c is null)
                {
                    continue;
                }

                if (StringComparer.InvariantCulture.Equals(name, c.Name))
                {
                    configuration = c;
                    break;
                }
            }

            return !(configuration is null);
        }
    }
}
