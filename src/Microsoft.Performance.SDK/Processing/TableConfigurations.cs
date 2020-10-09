// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Contains a collection of <see cref="TableConfiguration"/>s.
    /// </summary>
    public class TableConfigurations
        : IEnumerable<TableConfiguration>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TableConfigurations"/>
        ///     class.
        /// </summary>
        /// <param name="tableId">
        ///     The unique identifier of the table to which the configurations
        ///     apply.
        /// </param>
        public TableConfigurations(Guid tableId)
        {
            this.TableId = tableId;
        }

        /// <summary>
        ///     Gets the unique identifier of the table to which the configurations
        ///     apply.
        /// </summary>
        public Guid TableId { get; private set; }

        /// <summary>
        ///     Gets or sets the name of the default configuration.
        /// </summary>
        public string DefaultConfigurationName { get; set; }

        /// <summary>
        ///     Gets or sets the configurations in this instance.
        /// </summary>
        public IEnumerable<TableConfiguration> Configurations { get; set; }

        /// <inheritdoc />
        public IEnumerator<TableConfiguration> GetEnumerator()
        {
            return this.Configurations.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        ///     Checks the table Type for a <see cref="PrebuiltConfigurationsEmbeddedResourceAttribute"/> or a
        ///     <see cref="PrebuiltConfigurationsFilePathAttribute"/>, and deserializes the table configurations
        ///     if found.
        /// </summary>
        /// <param name="tableType">
        ///     Table type.
        /// </param>
        /// <param name="tableId">
        ///     Table id.
        /// </param>
        /// <param name="serializer">
        ///     Used to deserialize table data.
        /// </param>
        /// <returns>
        ///     TableConfigurations or null.
        /// </returns>
        public static TableConfigurations GetPrebuiltTableConfigurations(
            Type tableType,
            Guid tableId,
            ISerializer serializer)
        {
            return GetPrebuiltTableConfigurations(tableType, tableId, serializer, null);
        }

        /// <summary>
        ///     Checks the table Type for a <see cref="PrebuiltConfigurationsEmbeddedResourceAttribute"/> or a
        ///     <see cref="PrebuiltConfigurationsFilePathAttribute"/>, and deserializes the table configurations
        ///     if found.
        /// </summary>
        /// <param name="tableType">
        ///     Table type.
        /// </param>
        /// <param name="tableId">
        ///     Table id.
        /// </param>
        /// <param name="serializer">
        ///     Used to deserialize table data.
        /// </param>
        /// <param name="logger">
        ///     Used to log relevant messages.
        /// </param>
        /// <returns>
        ///     TableConfigurations or null.
        /// </returns>
        public static TableConfigurations GetPrebuiltTableConfigurations(
            Type tableType,
            Guid tableId,
            ISerializer serializer,
            ILogger logger)
        {
            Guard.NotNull(serializer, nameof(serializer));

            TableConfigurations tableConfigurations =
                GetPrebuiltConfigurationFromExternalFile(tableType, tableId, serializer, logger);

            if (tableConfigurations != null)
            {
                // todo:should we attempt to combine any embedded prebuilt configuration if both external and embedded attributes are set?
                return tableConfigurations;
            }

            tableConfigurations = GetPrebuiltConfigurationFromEmbeddedResource(tableType, tableId, serializer, logger);

            if (tableConfigurations != null)
            {
                return tableConfigurations;
            }

            tableConfigurations = new TableConfigurations(tableId);
            tableConfigurations.Configurations = Enumerable.Empty<TableConfiguration>();

            return tableConfigurations;
        }

        private static TableConfigurations GetPrebuiltConfigurationFromExternalFile(
            Type tableType,
            Guid tableId,
            ISerializer serializer,
            ILogger logger)
        {
            var tableAttribute = tableType.GetCustomAttribute<PrebuiltConfigurationsFilePathAttribute>();
            if (tableAttribute == null)
            {
                return null;
            }

            try
            {
                string resourcePath = tableAttribute.ExternalResourceFilePath;
                if (!File.Exists(resourcePath))
                {
                    string assemblyPath = Path.GetDirectoryName(tableType.Assembly.Location);
                    string fullPath = Path.Combine(assemblyPath, resourcePath);
                    if (!File.Exists(fullPath))
                    {
                        logger?.Warn(
                            $"Prebuilt table configuration file not found for table {tableType}: " +
                            $"{tableAttribute.ExternalResourceFilePath}.");
                        return null;
                    }
                    else
                    {
                        resourcePath = fullPath;
                    }
                }

                IEnumerable<TableConfigurations> tableConfigurations;
                using (var fileStream = File.Open(
                    resourcePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read))
                {
                    tableConfigurations = serializer.DeserializeTableConfigurations(fileStream, logger);
                }

                if (tableConfigurations == null)
                {
                    return null;
                }

                foreach (var tableConfiguration in tableConfigurations)
                {
                    if (tableConfiguration.TableId != tableId)
                    {
                        continue;
                    }

                    return tableConfiguration;
                }
            }
            catch (Exception e)
            {
                logger?.Warn(
                    $"Failed to processed table configurations for table {tableType} in file " +
                    $"{tableAttribute.ExternalResourceFilePath}: {e.Message}.");
            }

            return null;
        }

        private static TableConfigurations GetPrebuiltConfigurationFromEmbeddedResource(
            Type tableType,
            Guid tableId,
            ISerializer serializer,
            ILogger logger)
        {
            var tableAttribute = tableType.GetCustomAttribute<PrebuiltConfigurationsEmbeddedResourceAttribute>();
            if (tableAttribute == null)
            {
                return null;
            }

            try
            {
                var tableAssembly = Assembly.GetAssembly(tableType);
                Debug.Assert(tableAssembly != null, "Unable to retrieve assembly for a table type.");

                var resourceName = tableAssembly.GetManifestResourceNames()
                    .Single(name => name.EndsWith(tableAttribute.EmbeddedResourcePath));

                if (string.IsNullOrWhiteSpace(resourceName))
                {
                    logger?.Warn(
                        $"Unable to retrieve embedded resource for prebuilt configurations in table {tableType}.");
                    return null;
                }

                IEnumerable<TableConfigurations> tableConfigurations;
                using (var resourceStream = tableAssembly.GetManifestResourceStream(resourceName))
                {
                    tableConfigurations = serializer.DeserializeTableConfigurations(resourceStream);
                }

                if (tableConfigurations == null)
                {
                    return null;
                }

                foreach (var tableConfiguration in tableConfigurations)
                {
                    if (tableConfiguration.TableId != tableId)
                    {
                        continue;
                    }

                    return tableConfiguration;
                }
            }
            catch (Exception e)
            {
                logger?.Warn(
                    $"Unable to retrieve embedded resource for prebuilt configurations in table {tableType}\n" +
                    $"{e.Message}.");
            }

            return null;
        }
    }
}
