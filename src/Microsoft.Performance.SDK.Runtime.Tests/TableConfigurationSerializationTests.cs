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
using ColumnConfiguration = Microsoft.Performance.SDK.Runtime.DTO.Latest.ColumnConfiguration;
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

            var roles = typeof(ColumnRole).GetProperties()
                .Select(x => x.GetConstantValue())
                .Cast<string>()
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

        [TestMethod]
        [UnitTest]
        public void PreV1_ValidValuesSerialize()
        {
            var preV1config = BuildPreV1TableConfig();

            var prebuiltConfigs = new DTO.PreV1.PrebuiltConfigurations() { Tables = new[] { new DTO.PreV1.TableConfigurations() { Configurations = new[] { preV1config }, DefaultConfigurationName =preV1config.Name, TableId = Guid.NewGuid() } } };

            using (var stream = new MemoryStream())
            {
                TableConfigurationsSerializer.SerializeTableConfigurations(stream, prebuiltConfigs, null);
                stream.Seek(0, SeekOrigin.Begin);
                var configs = this.Sut.DeserializeTableConfigurations(stream).ToList();

                Assert.AreEqual(1, configs.Count);
                Assert.AreEqual(prebuiltConfigs.Tables[0].TableId, configs[0].TableId);

                Assert.IsNotNull(configs[0].Configurations);
                var configurations = configs[0].Configurations.ToList();
                Assert.AreEqual(1, configurations.Count);                

                var newConfig = configurations[0];
                Assert.IsNotNull(newConfig);

                Assert.AreEqual(preV1config.Name, newConfig.Name);
                Assert.AreEqual(preV1config.ChartType.ToString(), newConfig.ChartType.ToString());
                Assert.AreEqual(preV1config.AggregationOverTime.ToString(), newConfig.AggregationOverTime.ToString());
                Assert.AreEqual(preV1config.InitialFilterQuery, newConfig.InitialFilterQuery);
                Assert.AreEqual(preV1config.InitialExpansionQuery, newConfig.InitialExpansionQuery);
                Assert.AreEqual(preV1config.InitialSelectionQuery, newConfig.InitialSelectionQuery);
                Assert.AreEqual(preV1config.InitialFilterShouldKeep, newConfig.InitialFilterShouldKeep);
                Assert.AreEqual(preV1config.GraphFilterTopValue, newConfig.GraphFilterTopValue);
                Assert.AreEqual(preV1config.GraphFilterThresholdValue, newConfig.GraphFilterThresholdValue);
                Assert.AreEqual(preV1config.GraphFilterColumnName, newConfig.GraphFilterColumnName);
                Assert.AreEqual(preV1config.GraphFilterColumnGuid, newConfig.GraphFilterColumnGuid);
                Assert.AreEqual(preV1config.HelpText, newConfig.Description);                

                void CompareInnerValues<T, T2>(IEnumerable<T> select1, IEnumerable<T2> select2)
                {
                    CollectionAssert.AreEquivalent(select1.ToArray(), select2.ToArray());
                }

                Assert.AreEqual(preV1config.HighlightEntries.Count(), newConfig.HighlightEntries.Count());
                CompareInnerValues(preV1config.HighlightEntries.Select(x => x.DurationColumnGuid), newConfig.HighlightEntries.Select(x => x.DurationColumnGuid));
                CompareInnerValues(preV1config.HighlightEntries.Select(x => x.DurationColumnName), newConfig.HighlightEntries.Select(x => x.DurationColumnName));
                CompareInnerValues(preV1config.HighlightEntries.Select(x => x.EndTimeColumnGuid), newConfig.HighlightEntries.Select(x => x.EndTimeColumnGuid));
                CompareInnerValues(preV1config.HighlightEntries.Select(x => x.EndTimeColumnName), newConfig.HighlightEntries.Select(x => x.EndTimeColumnName));
                CompareInnerValues(preV1config.HighlightEntries.Select(x => x.HighlightColor), newConfig.HighlightEntries.Select(x => x.HighlightColor));
                CompareInnerValues(preV1config.HighlightEntries.Select(x => x.HighlightQuery), newConfig.HighlightEntries.Select(x => x.HighlightQuery));
                CompareInnerValues(preV1config.HighlightEntries.Select(x => x.StartTimeColumnGuid), newConfig.HighlightEntries.Select(x => x.StartTimeColumnGuid));
                CompareInnerValues(preV1config.HighlightEntries.Select(x => x.StartTimeColumnName), newConfig.HighlightEntries.Select(x => x.StartTimeColumnName));

                Assert.AreEqual(preV1config.Columns.Count(), newConfig.Columns.Count());
                CompareInnerValues(preV1config.Columns.Select(x => x.Metadata.Guid), newConfig.Columns.Select(x => x.Metadata.Guid));
                CompareInnerValues(preV1config.Columns.Select(x => x.Metadata.Name), newConfig.Columns.Select(x => x.Metadata.Name));
                CompareInnerValues(preV1config.Columns.Select(x => x.Metadata.Description), newConfig.Columns.Select(x => x.Metadata.Description));
                CompareInnerValues(preV1config.Columns.Select(x => x.Metadata.ShortDescription), newConfig.Columns.Select(x => x.Metadata.ShortDescription));
                CompareInnerValues(preV1config.Columns.Select(x => x.DisplayHints.AggregationMode.ToString()), newConfig.Columns.Select(x => x.DisplayHints.AggregationMode.ToString()));
                CompareInnerValues(preV1config.Columns.Select(x => x.DisplayHints.CellFormat), newConfig.Columns.Select(x => x.DisplayHints.CellFormat));
                CompareInnerValues(preV1config.Columns.Select(x => x.DisplayHints.IsVisible), newConfig.Columns.Select(x => x.DisplayHints.IsVisible));
                CompareInnerValues(preV1config.Columns.Select(x => x.DisplayHints.SortOrder.ToString()), newConfig.Columns.Select(x => x.DisplayHints.SortOrder.ToString()));
                CompareInnerValues(preV1config.Columns.Select(x => x.DisplayHints.SortPriority), newConfig.Columns.Select(x => x.DisplayHints.SortPriority));
                CompareInnerValues(preV1config.Columns.Select(x => x.DisplayHints.TextAlignment.ToString()), newConfig.Columns.Select(x => x.DisplayHints.TextAlignment.ToString()));
                CompareInnerValues(preV1config.Columns.Select(x => x.DisplayHints.Width), newConfig.Columns.Select(x => x.DisplayHints.Width));

                bool CheckEnum(DTO.PreV1.ColumnRole role)
                {
                    switch (role)
                    {
                        case DTO.PreV1.ColumnRole.Invalid:
                        case DTO.PreV1.ColumnRole.CountColumnMetadata:
                            return false;
                    }

                    return true;
                }

                // prune the roles that are invalid.
                var validRoles = new Dictionary<DTO.PreV1.ColumnRole, ColumnRoleEntry>(preV1config.ColumnRoles.Where(x => CheckEnum(x.Key)));

                Assert.AreEqual(validRoles.Count, newConfig.ColumnRoles.Count);
                foreach (var kvp in validRoles)
                {
                    string newRole = null;

                    switch (kvp.Key)
                    {
                        case DTO.PreV1.ColumnRole.StartTime:
                            newRole = ColumnRole.StartTime;
                            break;

                        case DTO.PreV1.ColumnRole.EndTime:
                            newRole = ColumnRole.EndTime;
                            break;

                        case DTO.PreV1.ColumnRole.Duration:
                            newRole = ColumnRole.Duration;
                            break;

                        case DTO.PreV1.ColumnRole.ResourceId:
                            newRole = ColumnRole.ResourceId;
                            break;

                        case DTO.PreV1.ColumnRole.RecLeft:
                            newRole = DTO.PreV1.ColumnRole.RecLeft.ToString();
                            break;

                        case DTO.PreV1.ColumnRole.RecTop:
                            newRole = DTO.PreV1.ColumnRole.RecTop.ToString();
                            break;

                        case DTO.PreV1.ColumnRole.RecHeight:
                            newRole = DTO.PreV1.ColumnRole.RecHeight.ToString();
                            break;

                        case DTO.PreV1.ColumnRole.RecWidth:
                            newRole = DTO.PreV1.ColumnRole.RecWidth.ToString();
                            break;

                        case DTO.PreV1.ColumnRole.StartThreadId:
                            newRole = DTO.PreV1.ColumnRole.StartThreadId.ToString();
                            break;

                        case DTO.PreV1.ColumnRole.EndThreadId:
                            newRole = DTO.PreV1.ColumnRole.EndThreadId.ToString();
                            break;

                        case DTO.PreV1.ColumnRole.HierarchicalTimeTree:
                            newRole = DTO.PreV1.ColumnRole.HierarchicalTimeTree.ToString();
                            break;

                        case DTO.PreV1.ColumnRole.WaitDuration:
                            newRole = DTO.PreV1.ColumnRole.WaitDuration.ToString();
                            break;

                        case DTO.PreV1.ColumnRole.WaitEndTime:
                            newRole = DTO.PreV1.ColumnRole.WaitEndTime.ToString();
                            break;
                    }

                    Assert.IsFalse(string.IsNullOrEmpty(newRole));

                    var newConfigPair = newConfig.ColumnRoles.First(x => x.Value == kvp.Value.ColumnGuid);
                    Assert.AreEqual(newRole, newConfigPair.Key);
                }
            }
        }

        private DTO.PreV1.TableConfiguration BuildPreV1TableConfig()
        {
            Func<string> testStr = () => Guid.NewGuid().ToString();

            return new DTO.PreV1.TableConfiguration()
            {
                Name = testStr(),
                Layout = DTO.PreV1.TableLayoutStyle.GraphAndTable,
                ChartType = DTO.Enums.ChartType.Flame,
                AggregationOverTime = DTO.Enums.AggregationOverTime.Current,
                InitialFilterQuery = testStr(),
                InitialExpansionQuery = testStr(),
                InitialSelectionQuery = testStr(),
                InitialFilterShouldKeep = true,
                GraphFilterTopValue = -1,
                GraphFilterThresholdValue = -1,
                GraphFilterColumnName = testStr(),
                GraphFilterColumnGuid = Guid.NewGuid(),
                HelpText = testStr(),
                HighlightEntries = Enumerable.Repeat(new DTO.HighlightEntry()
                {
                    DurationColumnGuid = Guid.NewGuid(),
                    DurationColumnName = testStr(),
                    EndTimeColumnGuid = Guid.NewGuid(),
                    EndTimeColumnName = testStr(),
                    HighlightColor = System.Drawing.Color.Red,
                    HighlightQuery = testStr(),
                    StartTimeColumnGuid = Guid.NewGuid(),
                    StartTimeColumnName = testStr()
                }, 3).ToArray(),
                Columns = Enumerable.Repeat(new DTO.V1_0.ColumnConfiguration()
                {
                    Metadata = new DTO.ColumnMetadata()
                    {
                        Name = testStr(),
                        Guid = Guid.NewGuid(),
                        Description = testStr(),
                        ShortDescription = testStr()
                    },
                    DisplayHints = new DTO.UIHints()
                    {
                        AggregationMode = DTO.Enums.AggregationMode.Count,
                        CellFormat = testStr(),
                        IsVisible = true,
                        SortOrder = DTO.Enums.SortOrder.Ascending,
                        SortPriority = -1,
                        TextAlignment = DTO.Enums.TextAlignment.Center,
                        Width = -1
                    }
                }, 3).ToArray(),
                ColumnRoles =
                new Dictionary<DTO.PreV1.ColumnRole, ColumnRoleEntry>(
                    Enum.GetValues(typeof(DTO.PreV1.ColumnRole))
                        .Cast<DTO.PreV1.ColumnRole>()
                        .Where(role => role != DTO.PreV1.ColumnRole.CountColumnMetadata)
                        .Select(role => new KeyValuePair<DTO.PreV1.ColumnRole, ColumnRoleEntry>(role, new ColumnRoleEntry() { ColumnName = testStr(), ColumnGuid = Guid.NewGuid() })))

            };
        }
    }
}
