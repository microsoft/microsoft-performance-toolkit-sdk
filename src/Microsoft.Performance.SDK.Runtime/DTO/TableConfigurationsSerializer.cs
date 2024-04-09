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
    ///     Used to de/serialize <see cref="TableConfigurations"/> instances from/to a stream.
    /// </summary>
    public class TableConfigurationsSerializer
        : ITableConfigurationsSerializer
    {
        private const string indentChars = "    ";

        private static Lazy<(double Version, Type PrebuiltConfigType)[]> versionToTypeLazy
            = new Lazy<(double Version, Type PrebuiltConfigType)[]>(() =>
            {
                var assembly = typeof(PrebuiltConfigurationsBase).Assembly;

                return assembly.GetTypes()
                               .Where(type => type.IsSubclassOf(typeof(PrebuiltConfigurationsBase)))
                               .Select(type => (((PrebuiltConfigurationsBase)Activator.CreateInstance(type)).Version, type)).ToArray();
            });

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
            else
            {
                startingPosition = stream.Position;
            }

            PrebuiltConfigurations prebuiltConfigurations = null;

            try
            {
                var baseSerializer = new DataContractJsonSerializer(typeof(PrebuiltConfigurationsBase));

                var baseObject = baseSerializer.ReadObject(stream) as PrebuiltConfigurationsBase;

                var deserailzeType = TableConfigurationsSerializer.versionToTypeLazy.Value.SingleOrDefault(x => x.Version == baseObject.Version);

                if (deserailzeType.PrebuiltConfigType == null)
                {
                    logger?.Warn($"Unsupported version: {baseObject.Version}");
                    return Enumerable.Empty<Processing.TableConfigurations>();
                }

                stream.Position = startingPosition;

                var serializer = new DataContractJsonSerializer(deserailzeType.PrebuiltConfigType);

                var desrailizedObject = serializer.ReadObject(stream);

                if (desrailizedObject is ISupportUpgrade<PrebuiltConfigurations> supportUpgrade)
                {
                    prebuiltConfigurations = supportUpgrade.Upgrade();
                }
                else if (desrailizedObject is PrebuiltConfigurations latestConfigs)
                {
                    prebuiltConfigurations = latestConfigs;
                }

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

            return ConvertDataTransferObjectsToTableConfigurations(prebuiltConfigurations.Tables);
        }

        /// <summary>
        ///     Serializes a <see cref="Processing.TableConfiguration"/> to a stream.
        /// </summary>
        /// <param name="stream">
        ///     Target stream.
        /// </param>
        /// <param name="tableConfiguration">
        ///     The <see cref="Processing.TableConfiguration"/> to serialize.
        /// </param>
        /// <param name="tableId">
        ///     Table identifier.
        /// </param>
        /// <remarks>
        ///     This will serialize a full <see cref="Processing.TableConfigurations"/> that has the given
        ///     <paramref name="tableId" /> as its <see cref="Processing.TableConfigurations.TableId"/>
        ///     and the given <paramref name="tableConfiguration"/> as its single element
        ///     <see cref="Processing.TableConfigurations.Configurations"/>.
        ///     The serialized <see cref="Processing.TableConfigurations"/> will have a null
        ///     <see cref="Processing.TableConfigurations.DefaultConfigurationName"/>.
        /// </remarks>
        public static void SerializeTableConfiguration(
            Stream stream,
            Processing.TableConfiguration tableConfiguration,
            Guid tableId)
        {
            SerializeTableConfiguration(stream, tableConfiguration, tableId, null);
        }

        /// <summary>
        ///     Serializes one or more <see cref="Processing.TableConfiguration"/> instances to a stream.
        /// </summary>
        /// <param name="stream">
        ///     Target stream.
        /// </param>
        /// <param name="tableConfigurations">
        ///     The <see cref="Processing.TableConfiguration"/> instances to serialize.
        /// </param>
        /// <param name="tableId">
        ///     Table identifier.
        /// </param>
        /// <remarks>
        ///     This will serialize a full <see cref="Processing.TableConfigurations"/> that has the given
        ///     <paramref name="tableId" /> as its <see cref="Processing.TableConfigurations.TableId"/>
        ///     and the given <paramref name="tableConfigurations"/> as its <see cref="Processing.TableConfigurations.Configurations"/>.
        ///     The serialized <see cref="Processing.TableConfigurations"/> will have a null
        ///     <see cref="Processing.TableConfigurations.DefaultConfigurationName"/>.
        /// </remarks>
        public static void SerializeTableConfigurations(
            Stream stream,
            Processing.TableConfiguration[] tableConfigurations,
            Guid tableId)
        {
            SerializeTableConfigurations(stream, tableConfigurations, tableId, null);
        }

        /// <summary>
        ///     Serializes a <see cref="Processing.TableConfiguration"/> to a stream.
        /// </summary>
        /// <param name="stream">
        ///     Target stream.
        /// </param>
        /// <param name="tableConfiguration">
        ///     The <see cref="Processing.TableConfiguration"/> to serialize.
        /// </param>
        /// <param name="tableId">
        ///     Table identifier.
        /// </param>
        /// <param name="logger">
        ///     Used to log relevant messages.
        /// </param>
        /// <remarks>
        ///     This will serialize a full <see cref="Processing.TableConfigurations"/> that has the given
        ///     <paramref name="tableId" /> as its <see cref="Processing.TableConfigurations.TableId"/>
        ///     and the given <paramref name="tableConfiguration"/> as its single element
        ///     <see cref="Processing.TableConfigurations.Configurations"/>.
        ///     The serialized <see cref="Processing.TableConfigurations"/> will have a null
        ///     <see cref="Processing.TableConfigurations.DefaultConfigurationName"/>.
        /// </remarks>
        public static void SerializeTableConfiguration(
            Stream stream,
            Processing.TableConfiguration tableConfiguration,
            Guid tableId,
            ILogger logger)
        {
            SerializeTableConfigurations(stream, new[] { tableConfiguration }, tableId, logger);
        }

        /// <summary>
        ///     Serializes one or more <see cref="Processing.TableConfiguration"/> instances to a stream.
        /// </summary>
        /// <param name="stream">
        ///     Target stream.
        /// </param>
        /// <param name="tableConfigurations">
        ///     The <see cref="Processing.TableConfiguration"/> instances to serialize.
        /// </param>
        /// <param name="tableId">
        ///     Table identifier.
        /// </param>
        /// <param name="logger">
        ///     Used to log relevant messages.
        /// </param>
        /// <remarks>
        ///     This will serialize a full <see cref="Processing.TableConfigurations"/> that has the given
        ///     <paramref name="tableId" /> as its <see cref="Processing.TableConfigurations.TableId"/>
        ///     and the given <paramref name="tableConfiguration"/> as its <see cref="Processing.TableConfigurations.Configurations"/>.
        ///     The serialized <see cref="Processing.TableConfigurations"/> will have a null
        ///     <see cref="Processing.TableConfigurations.DefaultConfigurationName"/>.
        /// </remarks>
        public static void SerializeTableConfigurations(
            Stream stream,
            Processing.TableConfiguration[] tableConfigurations,
            Guid tableId,
            ILogger logger)
        {
            var toSerialize = new Processing.TableConfigurations(tableId) { Configurations = tableConfigurations };
            SerializeTableConfigurations(stream, toSerialize, logger);
        }

        /// <summary>
        ///     Serializes <see cref="Processing.TableConfigurations"/> to a stream.
        /// </summary>
        /// <param name="stream">
        ///     Target stream.
        /// </param>
        /// <param name="tableConfigurations">
        ///     The <see cref="Processing.TableConfigurations"/> to serialize.
        /// </param>
        public static void SerializeTableConfigurations(
            Stream stream,
            Processing.TableConfigurations tableConfigurations)
        {
            SerializeTableConfigurations(stream, tableConfigurations, null);
        }

        /// <summary>
        ///     Serializes <see cref="Processing.TableConfigurations"/> to a stream.
        /// </summary>
        /// <param name="stream">
        ///     Target stream.
        /// </param>
        /// <param name="tableConfigurations">
        ///     The <see cref="Processing.TableConfigurations"/> to serialize.
        /// </param>
        /// <param name="logger">
        ///     Used to log relevant messages.
        /// </param>
        public static void SerializeTableConfigurations(
            Stream stream,
            Processing.TableConfigurations tableConfigurations,
            ILogger logger)
        {
            var prebuiltConfigurations = tableConfigurations.ConvertToDto();

            SerializeTableConfigurations(stream, prebuiltConfigurations, logger);
        }

        internal static void SerializeTableConfigurations<T>(
            Stream stream,
            T dtoPrebuiltConfigurations,
            ILogger logger)
            where T : PrebuiltConfigurationsBase
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
                    var serializer = new DataContractJsonSerializer(typeof(T));
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
