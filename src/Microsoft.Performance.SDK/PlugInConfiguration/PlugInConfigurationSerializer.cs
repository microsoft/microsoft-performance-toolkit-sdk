// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.PlugInConfiguration
{
    /// <summary>
    ///     Includes methods to de/serializing plug-in configurations.
    /// </summary>
    public static class PlugInConfigurationSerializer
    {
        public static readonly string DefaultFileName = "PlugInConfiguration.json";

        /// <summary>
        ///     Reads a configuration for the given custom data source.
        /// </summary>
        /// <param name="customDataSourceType">
        ///     Type of the custom data source.
        /// </param>
        /// <param name="logger">
        ///     Used to log any messages.
        /// </param>
        /// <returns>
        ///     A PlugInConfiguration on success; <c>null</c> on failure.
        /// </returns>
        public static PlugInConfiguration ReadFromDefaultFile(Type customDataSourceType, ILogger logger)
        {
            var assemblyPath = customDataSourceType.Assembly.Location;
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
        ///     A PlugInConfiguration on success; <c>null</c> on failure.
        /// </returns>
        public static PlugInConfiguration ReadFromDefaultFile(string plugInDirectory, ILogger logger)
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
        ///     A PlugInConfiguration on success; <c>null</c> on failure.
        /// </returns>
        public static PlugInConfiguration ReadFromStream(Stream configurationStream, ILogger logger)
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

            PlugInConfigurationDTO plugInConfigurationDTO;

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(PlugInConfigurationDTO));

                plugInConfigurationDTO = serializer.ReadObject(configurationStream) as PlugInConfigurationDTO;
                if (plugInConfigurationDTO == null)
                {
                    logger?.Error("Unable to deserialize Plug-In configuration.");
                    return null;
                }
            }
            catch (InvalidDataContractException e)
            {
                logger?.Error(e, "Invalid data contract - unable to load Plug-In configuration.");
                return null;
            }
            catch (SerializationException e)
            {
                logger?.Error(e, "Exception while deserializing Plug-In configuration.");
                return null;
            }

            return plugInConfigurationDTO.ConfigurationFromDTO(logger);
        }

        /// <summary>
        ///     Write a <see cref="PlugInConfiguration"/> to a stream.
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
        public static bool WriteToStream(PlugInConfiguration configuration, Stream configurationStream, ILogger logger)
        {
            Guard.NotNull(configurationStream, nameof(configurationStream));
            Guard.NotNull(configuration, nameof(configuration));

            if (!configurationStream.CanWrite)
            {
                throw new ArgumentException(message:
                    "Unable to write plug-in configuration. Cannot write to provided stream.",
                    paramName: nameof(configurationStream));
            }

            var outputConfiguration = configuration.ConfigurationToDTO();

            try
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
                    configurationStream, Encoding.Default, false, true, "    "))
                {
                    var serializer = new DataContractJsonSerializer(typeof(PlugInConfigurationDTO));
                    serializer.WriteObject(writer, outputConfiguration);
                }
            }
            catch (Exception e)
            {
                logger?.Error($"Failed to write Plug-In configuration: {e.Message}");
                return false;
            }

            configurationStream.SetLength(configurationStream.Position);
            configurationStream.Flush();

            return true;
        }
    }
}
