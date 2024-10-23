// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.DTO.V1_0
{
    [DataContract]
    internal class TableConfigurations
        : ISupportUpgrade<V1_3.TableConfigurations>
    {
        [DataMember(Order = 1)]
        public Guid TableId { get; set; }

        [DataMember(Order = 2)]
        public string DefaultConfigurationName { get; set; }

        [DataMember(Order = 3)]
        public TableConfiguration[] Configurations { get; set; }

        public V1_3.TableConfigurations Upgrade(ILogger logger)
        {

            var configs = this.Configurations.Select(config => config.Upgrade(logger)).ToArray();
            Guid? defaultGuid = GetDefaultConfigGuid(configs, logger);

            return new V1_3.TableConfigurations()
            {
                TableId = this.TableId,
                DefaultConfiguration = defaultGuid,
                Configurations = configs,
            };
        }

        private Guid? GetDefaultConfigGuid(V1_3.TableConfiguration[] configs, ILogger logger)
        {
            if (string.IsNullOrEmpty(this.DefaultConfigurationName))
            {
                return null;
            }

            List<V1_3.TableConfiguration> matchingDefault = configs.Where(c => c.Name.Equals(this.DefaultConfigurationName, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (matchingDefault.Count > 1)
            {
                logger.Warn($"Multiple configurations have the supplied default name '{this.DefaultConfigurationName}': using the first matching configuration.");
                return matchingDefault.First().Guid;
            }
            else if (matchingDefault.Count == 0)
            {
                return null;
            }
            else
            {
                return matchingDefault[0].Guid;
            }
        }
    }
}
