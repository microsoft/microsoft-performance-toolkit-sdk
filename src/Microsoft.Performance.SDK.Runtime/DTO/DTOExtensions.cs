// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Runtime.DTO.Enums;
using Microsoft.Performance.SDK.Runtime.DTO.V1_0;

namespace Microsoft.Performance.SDK.Runtime.DTO
{
    internal static class DTOExtensions
    {
        private static Func<Processing.TableConfiguration, IDictionary<string, ColumnRoleEntry>> defaultColumnRolesConverter = 
            (configuration) =>
            {
                var columnRoles = new Dictionary<string, ColumnRoleEntry>();
                foreach (var kvp in configuration.ColumnRoles)
                {
                    columnRoles[kvp.Key] = new ColumnRoleEntry()
                    {
                        ColumnGuid = kvp.Value,
                        ColumnName = configuration.Columns.FirstOrDefault(column => column.Metadata.Guid == kvp.Value)?.Metadata.Name,
                    };
                }

                return columnRoles;
            };

        internal static PrebuiltConfigurations ConvertToDto(
            this Processing.TableConfigurations tableConfigurations)
        {
            return tableConfigurations.ConvertToDto(defaultColumnRolesConverter);
        }

        internal static PrebuiltConfigurations ConvertToDto(
            this Processing.TableConfigurations tableConfigurations,
            Func<Processing.TableConfiguration, IDictionary<string, ColumnRoleEntry>> convertColumnRoles)
        {
            var dtoTableConfigurations = new PrebuiltConfigurations()
            {
                Version = PrebuiltConfigurations.DTOVersion,
                Tables = new TableConfigurations[]
               {
                    new TableConfigurations()
                    {
                        TableId = tableConfigurations.TableId,
                        Configurations = tableConfigurations.Select(config => config.ConvertToDto(convertColumnRoles)).ToArray(),
                        DefaultConfigurationName = tableConfigurations.DefaultConfigurationName
                    }
               }
            };

            return dtoTableConfigurations;
        }

        internal static Microsoft.Performance.SDK.Processing.TableConfigurations ConvertToSdk(this TableConfigurations dto)
        {
            var configurations = new List<Microsoft.Performance.SDK.Processing.TableConfiguration>();

            if (dto.Configurations != null)
            {
                foreach (var tableConfiguration in dto.Configurations)
                {
                    configurations.Add(ConvertToSdk(tableConfiguration));
                }
            }

            var tableConfigurations = new Microsoft.Performance.SDK.Processing.TableConfigurations(dto.TableId)
            {
                Configurations = configurations
            };

            if (!string.IsNullOrWhiteSpace(dto.DefaultConfigurationName))
            {
                tableConfigurations.DefaultConfigurationName = dto.DefaultConfigurationName;
            }

            return tableConfigurations;
        }

        private static Microsoft.Performance.SDK.Processing.TableConfiguration ConvertToSdk(this TableConfiguration dto)
        {
            var tableConfiguration = new Microsoft.Performance.SDK.Processing.TableConfiguration(dto.Name)
            {
                ChartType = dto.ChartType.ConvertToSdk(),
                AggregationOverTime = dto.AggregationOverTime.ConvertToSDK(),
                InitialFilterQuery = dto.InitialFilterQuery,
                InitialExpansionQuery = dto.InitialExpansionQuery,
                InitialSelectionQuery = dto.InitialSelectionQuery,
                InitialFilterShouldKeep = dto.InitialFilterShouldKeep,
                GraphFilterTopValue = dto.GraphFilterTopValue,
                GraphFilterThresholdValue = dto.GraphFilterThresholdValue,
                GraphFilterColumnName = dto.GraphFilterColumnName,
                GraphFilterColumnGuid = dto.GraphFilterColumnGuid,
                Description = dto.Description,
            };

            if (dto.HighlightEntries != null)
            {
                tableConfiguration.HighlightEntries = dto.HighlightEntries.Select(entry => entry.ConvertToSdk()).ToArray();
            }

            if (dto.Columns != null)
            {
                tableConfiguration.Columns = dto.Columns.Select(column => column.ConvertToSdk()).ToArray();
            }

            if (dto.ColumnRoles != null)
            {
                foreach (var kvp in dto.ColumnRoles)
                {
                    tableConfiguration.AddColumnRole(kvp.Key, kvp.Value.ColumnGuid);
                }
            }

            return tableConfiguration;
        }

        internal static TableConfiguration ConvertToDto(
            this Microsoft.Performance.SDK.Processing.TableConfiguration configuration,
            Func<Processing.TableConfiguration, IDictionary<string, ColumnRoleEntry>> convertColumnRoles)
        {
            var dto = new TableConfiguration
            {
                ChartType = configuration.ChartType.ConvertToDto(),
                AggregationOverTime = configuration.AggregationOverTime.ConvertToDto(),
                Columns = configuration.Columns.Select(column => column.ConvertToDto()).ToArray(),
                ColumnRoles = convertColumnRoles(configuration),
                Name = configuration.Name,
                InitialFilterQuery = configuration.InitialFilterQuery,
                InitialExpansionQuery = configuration.InitialExpansionQuery,
                InitialSelectionQuery = configuration.InitialSelectionQuery,
                InitialFilterShouldKeep = configuration.InitialFilterShouldKeep,
                GraphFilterTopValue = configuration.GraphFilterTopValue,
                GraphFilterThresholdValue = configuration.GraphFilterThresholdValue,
                GraphFilterColumnName = configuration.GraphFilterColumnName,
                GraphFilterColumnGuid = configuration.GraphFilterColumnGuid,
                Description = configuration.Description,
                HighlightEntries = configuration.HighlightEntries.Select(entry => entry.ConvertToDto()).ToArray(),
            };

            return dto;
        }

        private static Microsoft.Performance.SDK.Processing.AggregationMode ConvertToSdk(this AggregationMode dtoEnum)
        {
            switch (dtoEnum)
            {
                case AggregationMode.None:
                    return Microsoft.Performance.SDK.Processing.AggregationMode.None;
                case AggregationMode.Average:
                    return Microsoft.Performance.SDK.Processing.AggregationMode.Average;
                case AggregationMode.Sum:
                    return Microsoft.Performance.SDK.Processing.AggregationMode.Sum;
                case AggregationMode.Count:
                    return Microsoft.Performance.SDK.Processing.AggregationMode.Count;
                case AggregationMode.Min:
                    return Microsoft.Performance.SDK.Processing.AggregationMode.Min;
                case AggregationMode.Max:
                    return Microsoft.Performance.SDK.Processing.AggregationMode.Max;
                case AggregationMode.UniqueCount:
                    return Microsoft.Performance.SDK.Processing.AggregationMode.UniqueCount;
                case AggregationMode.Peak:
                    return Microsoft.Performance.SDK.Processing.AggregationMode.Peak;
                case AggregationMode.WeightedAverage:
                    return Microsoft.Performance.SDK.Processing.AggregationMode.WeightedAverage;
                default:
                    //Looks like an unsupported version of the DTO, we shouldn't have got here
                    throw new InvalidOperationException();
            }
        }

        private static AggregationMode ConvertToDto(this Microsoft.Performance.SDK.Processing.AggregationMode aggregationMode)
        {
            switch (aggregationMode)
            {
                case Microsoft.Performance.SDK.Processing.AggregationMode.None:
                    return AggregationMode.None;
                case Microsoft.Performance.SDK.Processing.AggregationMode.Average:
                    return AggregationMode.Average;
                case Microsoft.Performance.SDK.Processing.AggregationMode.Sum:
                    return AggregationMode.Sum;
                case Microsoft.Performance.SDK.Processing.AggregationMode.Count:
                    return AggregationMode.Count;
                case Microsoft.Performance.SDK.Processing.AggregationMode.Min:
                    return AggregationMode.Min;
                case Microsoft.Performance.SDK.Processing.AggregationMode.Max:
                    return AggregationMode.Max;
                case Microsoft.Performance.SDK.Processing.AggregationMode.UniqueCount:
                    return AggregationMode.UniqueCount;
                case Microsoft.Performance.SDK.Processing.AggregationMode.Peak:
                    return AggregationMode.Peak;
                case Microsoft.Performance.SDK.Processing.AggregationMode.WeightedAverage:
                    return AggregationMode.WeightedAverage;
                default:
                    // This needs to be updated to support the new enum option.
                    throw new InvalidOperationException();
            }
        }

        private static Microsoft.Performance.SDK.Processing.ChartType ConvertToSdk(this ChartType dtoEnum)
        {
            switch (dtoEnum)
            {
                case ChartType.Line:
                    return Microsoft.Performance.SDK.Processing.ChartType.Line;
                case ChartType.StackedLine:
                    return Microsoft.Performance.SDK.Processing.ChartType.StackedLine;
                case ChartType.StackedBars:
                    return Microsoft.Performance.SDK.Processing.ChartType.StackedBars;
                case ChartType.StateDiagram:
                    return Microsoft.Performance.SDK.Processing.ChartType.StateDiagram;
                case ChartType.PointInTime:
                    return Microsoft.Performance.SDK.Processing.ChartType.PointInTime;
                case ChartType.Flame:
                    return Microsoft.Performance.SDK.Processing.ChartType.Flame;
                default:
                    //Looks like an unsupported version of the DTO, we shouldn't have got here
                    throw new InvalidOperationException();
            }
        }

        private static ChartType ConvertToDto(this Microsoft.Performance.SDK.Processing.ChartType chartType)
        {
            switch (chartType)
            {
                case Microsoft.Performance.SDK.Processing.ChartType.Line:
                    return ChartType.Line;
                case Microsoft.Performance.SDK.Processing.ChartType.StackedLine:
                    return ChartType.StackedLine;
                case Microsoft.Performance.SDK.Processing.ChartType.StackedBars:
                    return ChartType.StackedBars;
                case Microsoft.Performance.SDK.Processing.ChartType.StateDiagram:
                    return ChartType.StateDiagram;
                case Microsoft.Performance.SDK.Processing.ChartType.PointInTime:
                    return ChartType.PointInTime;
                case Microsoft.Performance.SDK.Processing.ChartType.Flame:
                    return ChartType.Flame;
                default:
                    // This needs to be updated to support the new enum option.
                    throw new InvalidOperationException();
            }
        }        

        private static Microsoft.Performance.SDK.Processing.AggregationOverTime ConvertToSDK(this AggregationOverTime dtoEnum)
        {
            switch (dtoEnum)
            {
                case AggregationOverTime.Current:
                    return Microsoft.Performance.SDK.Processing.AggregationOverTime.Current;
                case AggregationOverTime.Rate:
                    return Microsoft.Performance.SDK.Processing.AggregationOverTime.Rate;
                case AggregationOverTime.Cumulative:
                    return Microsoft.Performance.SDK.Processing.AggregationOverTime.Cumulative;
                case AggregationOverTime.Outstanding:
                    return Microsoft.Performance.SDK.Processing.AggregationOverTime.Outstanding;
                case AggregationOverTime.OutstandingPeak:
                    return Microsoft.Performance.SDK.Processing.AggregationOverTime.OutstandingPeak;
                default:
                    //Looks like an unsupported version of the DTO, we shouldn't have got here
                    throw new InvalidOperationException();
            }
        }

        private static AggregationOverTime ConvertToDto(this Microsoft.Performance.SDK.Processing.AggregationOverTime aggregationOverTime)
        {
            switch (aggregationOverTime)
            {
                case Microsoft.Performance.SDK.Processing.AggregationOverTime.Current:
                    return AggregationOverTime.Current;
                case Microsoft.Performance.SDK.Processing.AggregationOverTime.Rate:
                    return AggregationOverTime.Rate;
                case Microsoft.Performance.SDK.Processing.AggregationOverTime.Cumulative:
                    return AggregationOverTime.Cumulative;
                case Microsoft.Performance.SDK.Processing.AggregationOverTime.Outstanding:
                    return AggregationOverTime.Outstanding;
                case Microsoft.Performance.SDK.Processing.AggregationOverTime.OutstandingPeak:
                    return AggregationOverTime.OutstandingPeak;
                default:
                    // This needs to be updated to support the new enum option.
                    throw new InvalidOperationException();
            }
        }

        private static Microsoft.Performance.SDK.Processing.SortOrder ConvertToSdk(this SortOrder dtoEnum)
        {
            switch (dtoEnum)
            {
                case SortOrder.None:
                    return Microsoft.Performance.SDK.Processing.SortOrder.None;
                case SortOrder.Ascending:
                    return Microsoft.Performance.SDK.Processing.SortOrder.Ascending;
                case SortOrder.Descending:
                    return Microsoft.Performance.SDK.Processing.SortOrder.Descending;
                case SortOrder.Ascending_Abs:
                    return Microsoft.Performance.SDK.Processing.SortOrder.Ascending_Abs;
                case SortOrder.Descending_Abs:
                    return Microsoft.Performance.SDK.Processing.SortOrder.Descending_Abs;
                default:
                    //Looks like an unsupported version of the DTO, we shouldn't have got here
                    throw new InvalidOperationException();
            }
        }

        private static SortOrder ConvertToDto(this Microsoft.Performance.SDK.Processing.SortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case Microsoft.Performance.SDK.Processing.SortOrder.None:
                    return SortOrder.None;
                case Microsoft.Performance.SDK.Processing.SortOrder.Ascending:
                    return SortOrder.Ascending;
                case Microsoft.Performance.SDK.Processing.SortOrder.Descending:
                    return SortOrder.Descending;
                case Microsoft.Performance.SDK.Processing.SortOrder.Ascending_Abs:
                    return SortOrder.Ascending_Abs;
                case Microsoft.Performance.SDK.Processing.SortOrder.Descending_Abs:
                    return SortOrder.Descending_Abs;
                default:
                    // This needs to be updated to support the new enum option.
                    throw new InvalidOperationException();
            }
        }

        private static Microsoft.Performance.SDK.Processing.TextAlignment ConvertToSdk(this TextAlignment dtoEnum)
        {
            switch (dtoEnum)
            {
                case TextAlignment.Left:
                    return Microsoft.Performance.SDK.Processing.TextAlignment.Left;
                case TextAlignment.Right:
                    return Microsoft.Performance.SDK.Processing.TextAlignment.Right;
                case TextAlignment.Center:
                    return Microsoft.Performance.SDK.Processing.TextAlignment.Center;
                case TextAlignment.Justify:
                    return Microsoft.Performance.SDK.Processing.TextAlignment.Justify;
                default:
                    //Looks like an unsupported version of the DTO, we shouldn't have got here
                    throw new InvalidOperationException();
            }
        }

        private static TextAlignment ConvertToDto(this Microsoft.Performance.SDK.Processing.TextAlignment textAlignment)
        {
            switch (textAlignment)
            {
                case Microsoft.Performance.SDK.Processing.TextAlignment.Left:
                    return TextAlignment.Left;
                case Microsoft.Performance.SDK.Processing.TextAlignment.Right:
                    return TextAlignment.Right;
                case Microsoft.Performance.SDK.Processing.TextAlignment.Center:
                    return TextAlignment.Center;
                case Microsoft.Performance.SDK.Processing.TextAlignment.Justify:
                    return TextAlignment.Justify;
                default:
                    // This needs to be updated to support the new enum option.
                    throw new InvalidOperationException();
            }
        }

        private static Processing.ColumnMetadata ConvertToSdk(this ColumnMetadata dto)
        {
            return new Processing.ColumnMetadata(dto.Guid, dto.Name, dto.Description) { ShortDescription = dto.ShortDescription };
        }

        private static ColumnMetadata ConvertToDto(this Processing.ColumnMetadata columnMetadata)
        {
            var dto = new ColumnMetadata
            {
                Guid = columnMetadata.Guid,
                Name = columnMetadata.Name,
                Description = columnMetadata.Description,
                ShortDescription = columnMetadata.ShortDescription,
            };

            return dto;
        }

        private static Microsoft.Performance.SDK.Processing.UIHints ConvertToSdk(this UIHints dto)
        {
            var uiHints = new Microsoft.Performance.SDK.Processing.UIHints
            {
                IsVisible = dto.IsVisible,
                TextAlignment = dto.TextAlignment.ConvertToSdk(),
                Width = dto.Width,
                SortOrder = dto.SortOrder.ConvertToSdk(),
                SortPriority = dto.SortPriority,
                AggregationMode = dto.AggregationMode.ConvertToSdk(),
                CellFormat = dto.CellFormat,
            };

            return uiHints;
        }

        private static UIHints ConvertToDto(this Microsoft.Performance.SDK.Processing.UIHints uiHints)
        {
            var dto = new UIHints
            {
                IsVisible = uiHints.IsVisible,
                TextAlignment = uiHints.TextAlignment.ConvertToDto(),
                Width = uiHints.Width,
                SortOrder = uiHints.SortOrder.ConvertToDto(),
                SortPriority = uiHints.SortPriority,
                AggregationMode = uiHints.AggregationMode.ConvertToDto(),
                CellFormat = uiHints.CellFormat,
            };

            return dto;
        }

        private static Microsoft.Performance.SDK.Processing.ColumnConfiguration ConvertToSdk(
            this ColumnConfiguration dto)
        {
            // These are checked by reference, so they need to point to these instances.
            if (dto.Metadata.Guid == Microsoft.Performance.SDK.Processing.TableConfiguration.PivotColumn.Metadata.Guid)
            {
                return Microsoft.Performance.SDK.Processing.TableConfiguration.PivotColumn;
            }
            else if (dto.Metadata.Guid == Microsoft.Performance.SDK.Processing.TableConfiguration.LeftFreezeColumn.Metadata.Guid)
            {
                return Microsoft.Performance.SDK.Processing.TableConfiguration.LeftFreezeColumn;
            }
            else if (dto.Metadata.Guid == Microsoft.Performance.SDK.Processing.TableConfiguration.RightFreezeColumn.Metadata.Guid)
            {
                return Microsoft.Performance.SDK.Processing.TableConfiguration.RightFreezeColumn;
            }
            else if (dto.Metadata.Guid == Microsoft.Performance.SDK.Processing.TableConfiguration.GraphColumn.Metadata.Guid)
            {
                return Microsoft.Performance.SDK.Processing.TableConfiguration.GraphColumn;
            }

            var columnConfiguration = new Microsoft.Performance.SDK.Processing.ColumnConfiguration(
                    dto.Metadata.ConvertToSdk(),
                    dto.DisplayHints.ConvertToSdk());
            return columnConfiguration;
        }

        private static ColumnConfiguration ConvertToDto(
            this Microsoft.Performance.SDK.Processing.ColumnConfiguration columnConfiguration)
        {
            var dto = new ColumnConfiguration
            {
                DisplayHints = columnConfiguration.DisplayHints.ConvertToDto(),
                Metadata = columnConfiguration.Metadata.ConvertToDto(),
            };

            return dto;
        }        

        private static Microsoft.Performance.SDK.Processing.HighlightEntry ConvertToSdk(
            this HighlightEntry dto)
        {
            var highlightEntry = new Microsoft.Performance.SDK.Processing.HighlightEntry
            {
                StartTimeColumnName = dto.StartTimeColumnName,
                StartTimeColumnGuid = dto.StartTimeColumnGuid,
                EndTimeColumnName = dto.EndTimeColumnName,
                EndTimeColumnGuid = dto.EndTimeColumnGuid,
                DurationColumnName = dto.DurationColumnName,
                DurationColumnGuid = dto.DurationColumnGuid,
                HighlightQuery = dto.HighlightQuery,
                HighlightColor = dto.HighlightColor,
            };
                    
            return highlightEntry;
        }

        private static HighlightEntry ConvertToDto(
            this Microsoft.Performance.SDK.Processing.HighlightEntry highlightEntry)
        {
            var dto = new HighlightEntry
            {
                StartTimeColumnName = highlightEntry.StartTimeColumnName,
                StartTimeColumnGuid = highlightEntry.StartTimeColumnGuid,
                EndTimeColumnName = highlightEntry.EndTimeColumnName,
                EndTimeColumnGuid = highlightEntry.EndTimeColumnGuid,
                DurationColumnName = highlightEntry.DurationColumnName,
                DurationColumnGuid = highlightEntry.DurationColumnGuid,
                HighlightQuery = highlightEntry.HighlightQuery,
                HighlightColor = highlightEntry.HighlightColor,
            };

            return dto;
        }
    }
}
