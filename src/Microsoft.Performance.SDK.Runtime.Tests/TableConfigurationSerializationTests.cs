// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.DTO;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TableConfiguration = Microsoft.Performance.SDK.Processing.TableConfiguration;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class TableConfigurationSerializationTests
    {
        private Guid TableGuid { get; set; }

        private TableConfigurationsSerializer Sut { get; set; }

        [TestInitialize]
        public void Setup()
        {
            this.TableGuid = Guid.NewGuid();
            this.Sut = new TableConfigurationsSerializer();
        }

        [TestMethod]
        [UnitTest]
        public void ColumnRoles_EmptySerializes()
        {
            var c1 = Any.ColumnConfiguration();
            var c2 = Any.ColumnConfiguration();
            var c3 = Any.ColumnConfiguration();
            var c4 = Any.ColumnConfiguration();
            var c5 = Any.ColumnConfiguration();

            var config = new TableConfiguration("test")
            {
                Columns = new[] { c1, c2, c3, c4, c5, },
            };

            using (var stream = new MemoryStream())
            {
                TableConfigurationsSerializer.SerializeTableConfiguration(stream, config, this.TableGuid);
                stream.Seek(0, SeekOrigin.Begin);
                var roundTripped = this.Sut.DeserializeTableConfigurations(stream).ToList();

                Assert.AreEqual(1, roundTripped.Count);

                Assert.IsNotNull(roundTripped[0].Configurations);
                var configurations = roundTripped[0].Configurations.ToList();
                Assert.AreEqual(1, configurations.Count);

                var roundTrippedConfig = configurations[0];
                Assert.IsNotNull(roundTrippedConfig);

                Assert.AreEqual(0, roundTrippedConfig.ColumnRoles.Count);
            }
        }

        [TestMethod]
        [UnitTest]
        public void ColumnRoles_ValidValuesSerialize()
        {
            var config = new TableConfiguration("test");

            var roles = Enum.GetValues(typeof(ColumnRole))
                .Cast<ColumnRole>()
                .Where(x => x.IsValidColumnRole())
                .ToList();
            var columns = Enumerable.Range(0, roles.Count).Select(_ => Any.ColumnConfiguration()).ToList();
            config.Columns = columns;

            for (var i = 0; i < roles.Count; ++i)
            {
                var role = roles[i];
                var column = columns[i];
                config.AddColumnRole(role, column.Metadata.Guid);
            }

            using (var stream = new MemoryStream())
            {
                TableConfigurationsSerializer.SerializeTableConfiguration(stream, config, this.TableGuid);
                stream.Seek(0, SeekOrigin.Begin);
                var roundTripped = this.Sut.DeserializeTableConfigurations(stream).ToList();

                Assert.AreEqual(1, roundTripped.Count);

                Assert.IsNotNull(roundTripped[0].Configurations);
                var configurations = roundTripped[0].Configurations.ToList();
                Assert.AreEqual(1, configurations.Count);

                var roundTrippedConfig = configurations[0];
                Assert.IsNotNull(roundTrippedConfig);

                Assert.AreEqual(config.ColumnRoles.Count, roundTrippedConfig.ColumnRoles.Count);
                foreach(var kvp in config.ColumnRoles)
                {
                    Assert.IsTrue(roundTrippedConfig.ColumnRoles.ContainsKey(kvp.Key));
                    Assert.AreEqual(
                        kvp.Value,
                        roundTrippedConfig.ColumnRoles[kvp.Key]);
                }
            }
        }
    }
}
