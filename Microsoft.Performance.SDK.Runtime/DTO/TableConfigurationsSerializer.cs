// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    /// <summary>
    ///     Used to de/serialize table configurations from/to a stream.
    /// </summary>
    public class TableConfigurationsSerializer
        : ISerializer
    {
        private const string indentChars = "    ";

        /// <inheritdoc/>
        public IEnumerable<Processing.TableConfigurations> DeserializeTableConfigurations(Stream stream)
        {
            return DeserializeTableConfigurations(stream, null);
        }

        /// <inheritdoc/>
        public IEnumerable<Processing.TableConfigurations> DeserializeTableConfigurations(Stream stream, ILogger logger)
        {
            if (stream == null)
            {
                return Enumerable.Empty<Processing.TableConfigurations>();
            }

            var skipByteOrderMark = false;
            var startingPosition = stream.Position;
            using (var binaryReader = new BinaryReader(stream, Encoding.UTF8, true))
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
                stream.Position = startingPosition;
            }

            PrebuiltConfigurations prebuiltConfigurations = null;

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(PrebuiltConfigurations));

                prebuiltConfigurations = serializer.ReadObject(stream) as PrebuiltConfigurations;
                if (prebuiltConfigurations == null)
                {
                    return Enumerable.Empty<Processing.TableConfigurations>();
                }
            }
            catch (InvalidDataContractException e)
            {
                logger?.Warn($"Invalid {nameof(Processing.TableConfiguration)} data found: {e.Message}");
                return Enumerable.Empty<Processing.TableConfigurations>();
            }

            catch (SerializationException e)
            {
                logger?.Warn(
                    $"An exception was encountered while deserializing {nameof(Processing.TableConfiguration)}: " +
                    $"{e.Message}");
                return Enumerable.Empty<Processing.TableConfigurations>();
            }

            // _CDS_
            // todo:validate the version

            return ConvertDataTransferObjectsToTableConfigurations(prebuiltConfigurations.Tables);
        }

        /// <summary>
        ///     Serializes a table configuration to a stream.
        /// </summary>
        /// <param name="stream">
        ///     Target stream.
        /// </param>
        /// <param name="tableConfiguration">
        ///     Table configuration to serialize.
        /// </param>
        /// <param name="tableId">
        ///     Table identifier.
        /// </param>
        public static void SerializeTableConfiguration(
            Stream stream,
            Processing.TableConfiguration tableConfiguration,
            Guid tableId)
        {
            SerializeTableConfiguration(stream, tableConfiguration, tableId, null);
        }

        /// <summary>
        ///     Serializes a table configuration to a stream.
        /// </summary>
        /// <param name="stream">
        ///     Target stream.
        /// </param>
        /// <param name="tableConfiguration">
        ///     Table configuration to serialize.
        /// </param>
        /// <param name="tableId">
        ///     Table identifier.
        /// </param>
        /// <param name="logger">
        ///     Used to log relevant messages.
        /// </param>
        public static void SerializeTableConfiguration(
            Stream stream,
            Processing.TableConfiguration tableConfiguration,
            Guid tableId,
            ILogger logger)
        {
            var tableConfigurations = new Processing.TableConfigurations(tableId) { Configurations = new[] { tableConfiguration } };
            var prebuiltConfigurations = tableConfigurations.ConvertToDto();

            SerializeTableConfigurations(stream, prebuiltConfigurations, logger);
        }

        private static void SerializeTableConfigurations(
            Stream stream,
            PrebuiltConfigurations dtoPrebuiltConfigurations,
            ILogger logger)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            try
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
                        stream, Encoding.Default, false, true, indentChars))
                {
                    var serializer = new DataContractJsonSerializer(typeof(PrebuiltConfigurations));
                    serializer.WriteObject(writer, dtoPrebuiltConfigurations);
                }
            }
            catch (Exception exception)
            {
                logger?.Warn($"Failed to serialize table configurations: {exception.Message}");
            }
        }

        private static IEnumerable<Processing.TableConfigurations> ConvertDataTransferObjectsToTableConfigurations(
            TableConfigurations[] transferObjects)
        {
            var tableConfigurations = new List<Processing.TableConfigurations>(transferObjects.Length);

            foreach (var transferObject in transferObjects)
            {
                tableConfigurations.Add(transferObject.ConvertToSdk());
            }

            return tableConfigurations;
        }
    }
}
