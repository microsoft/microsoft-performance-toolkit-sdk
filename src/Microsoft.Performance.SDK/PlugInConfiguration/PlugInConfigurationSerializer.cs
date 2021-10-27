// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.PluginConfiguration
{
    /// <summary>
    ///     Includes methods to de/serializing plugin configurations.
    /// </summary>
    public static class PluginConfigurationSerializer
    {
        /// <summary>
        ///     The default file name for a plugin configuration.
        /// </summary>
        /// <remarks>
        ///     This does not follow the standard lowercase "plugin" case-convention
        ///     in order to remain backwards compatible with previous plugin configurations.
        /// </remarks>
        public static readonly string DefaultFileName = "PlugInConfiguration.json";

        /// <summary>
        ///     Reads a configuration for the given <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="processingSourceType">
        ///     Type of the <see cref="IProcessingSource"/>.
        /// </param>
        /// <param name="logger">
        ///     Used to log any messages.
        /// </param>
        /// <returns>
        ///     A <see cref="PluginConfiguration"/> on success; <c>null</c> on failure.
        /// </returns>
        public static PluginConfiguration ReadFromDefaultFile(Type processingSourceType, ILogger logger)
        {
            var assemblyPath = processingSourceType.Assembly.Location;
            var directory = Path.GetDirectoryName(assemblyPath);

            return ReadFromDefaultFile(directory, logger);
        }

        /// <summary>
        ///     Reads a configuration from a given directory.
        /// </summary>
        /// <param name="plugInDirectory">
        ///     The directory with the configuration file.
        /// </param>
        /// <param name="logger">
        ///     Used to log any errors.
        /// </param>
        /// <returns>
        ///     A <see cref="PluginConfiguration"/> on success; <c>null</c> on failure.
        /// </returns>
        public static PluginConfiguration ReadFromDefaultFile(string plugInDirectory, ILogger logger)
        {
            var pathToDefaultConfiguration = Path.Combine(plugInDirectory, DefaultFileName);
            if (!File.Exists(pathToDefaultConfiguration))
            {
                throw new ArgumentException(
                    message: $"The default configuration file is not found: {pathToDefaultConfiguration}",
                    paramName: nameof(plugInDirectory));
            }

            using (var fileStream = File.OpenRead(pathToDefaultConfiguration))
                return ReadFromStream(fileStream, logger);
        }

        /// <summary>
        ///     Reads a configuration from a given stream.
        /// </summary>
        /// <param name="configurationStream">
        ///     Source of the configuration data.
        /// </param>
        /// <param name="logger">
        ///     Used to log any errors.
        /// </param>
        /// <returns>
        ///     A <see cref="PluginConfiguration"/> on success; <c>null</c> on failure.
        /// </returns>
        public static PluginConfiguration ReadFromStream(Stream configurationStream, ILogger logger)
        {
            Guard.NotNull(configurationStream, nameof(configurationStream));

            if (configurationStream.CanSeek)
            {
                var skipByteOrderMark = false;
                var startingPosition = configurationStream.Position;
                using (var binaryReader = new BinaryReader(configurationStream, Encoding.UTF8, true))
                {
                    var byte1 = binaryReader.ReadByte();
                    if (byte1 == 0xEF)
                    {
                        var byte2 = binaryReader.ReadByte();
                        if (byte2 == 0xBB)
                        {
                            var byte3 = binaryReader.ReadByte();
                            if (byte3 == 0xBF)
                            {
                                skipByteOrderMark = true;
                            }
                        }
                    }
                }

                if (!skipByteOrderMark)
                {
                    configurationStream.Position = startingPosition;
                }
            }

            PluginConfigurationDTO plugInConfigurationDTO;

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(PluginConfigurationDTO));

                plugInConfigurationDTO = serializer.ReadObject(configurationStream) as PluginConfigurationDTO;
                if (plugInConfigurationDTO == null)
                {
                    logger?.Error("Unable to deserialize plugin configuration.");
                    return null;
                }
            }
            catch (InvalidDataContractException e)
            {
                logger?.Error(e, "Invalid data contract - unable to load plugin configuration.");
                return null;
            }
            catch (SerializationException e)
            {
                logger?.Error(e, "Exception while deserializing plugin configuration.");
                return null;
            }

            return plugInConfigurationDTO.ConfigurationFromDTO(logger);
        }

        /// <summary>
        ///     Write a <see cref="PluginConfiguration"/> to a stream.
        /// </summary>
        /// <param name="configuration">
        ///     Configuration to write.
        /// </param>
        /// <param name="configurationStream">
        ///     Stream to write to.
        /// </param>
        /// <param name="logger">
        ///     Used to log any errors.
        /// </param>
        /// <returns>
        ///     <c>true</c> when written successfully; <c>false</c> otherwise.
        /// </returns>
        public static bool WriteToStream(PluginConfiguration configuration, Stream configurationStream, ILogger logger)
        {
            Guard.NotNull(configurationStream, nameof(configurationStream));
            Guard.NotNull(configuration, nameof(configuration));

            if (!configurationStream.CanWrite)
            {
                throw new ArgumentException(message:
                    "Unable to write plugin configuration. Cannot write to provided stream.",
                    paramName: nameof(configurationStream));
            }

            var outputConfiguration = configuration.ConfigurationToDTO();

            try
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
                    configurationStream, Encoding.Default, false, true, "    "))
                {
                    var serializer = new DataContractJsonSerializer(typeof(PluginConfigurationDTO));
                    serializer.WriteObject(writer, outputConfiguration);
                }
            }
            catch (Exception e)
            {
                logger?.Error($"Failed to write plugin configuration: {e.Message}");
                return false;
            }

            configurationStream.SetLength(configurationStream.Position);
            configurationStream.Flush();

            return true;
        }
    }
}
