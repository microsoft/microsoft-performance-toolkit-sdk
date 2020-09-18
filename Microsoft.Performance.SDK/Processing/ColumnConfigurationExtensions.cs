// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting with
    ///     <see cref="ColumnConfiguration"/> instances.
    /// </summary>
    public static class ColumnConfigurationExtensions
    {
        /// <summary>
        ///     Sets the format provider for the given configuration.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="ColumnConfiguration"/> to be modified.
        /// </param>
        /// <param name="formatProvider">
        ///     The new <see cref="IFormatProvider"/> for the configuration.
        ///     This parameter may be <c>null</c>.
        /// </param>
        public static void SetFormatProvider(
            this ColumnConfiguration self,
            IFormatProvider formatProvider)
        {
            self.Metadata.FormatProvider = formatProvider;
        }

        /// <summary>
        ///     Creates a new <see cref="ColumnConfiguration"/> that
        ///     is a clone of the given ColumnConfiguration, except
        ///     for the name projection, which is replaced by the given
        ///     name projection.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="ColumnConfiguration"/> to be modified.
        /// </param>
        /// <param name="nameProjection">
        ///     The new name projection.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="nameProjection"/> is <c>null</c>.
        /// </exception>
        public static ColumnConfiguration WithDynamicName(
            this ColumnConfiguration self,
            IProjection<int, string> nameProjection)
        {
            var newConfiguration = new ColumnConfiguration(
                new ColumnMetadata(
                    self.Metadata.Guid,
                    self.Metadata.Name,
                    nameProjection,
                    self.Metadata.Description),
                self.DisplayHints);
            return newConfiguration;
        }

        /// <summary>
        ///     Determines if the given <see cref="ColumnConfiguration"/>
        ///     is considered to be a metadata column. The metadata columns
        ///     are found in the <see cref="TableConfiguration"/> class,
        ///     for example <see cref="TableConfiguration.PivotColumn"/>.
        /// </summary>
        /// <param name="self">
        ///     The column to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="self"/> is one of the <see cref="TableConfiguration"/>
        ///     metadata columns; <c>false</c> otherwise.
        /// </returns>
        public static bool IsMetadataColumn(this ColumnConfiguration self)
        {
            if (self is null)
            {
                return false;
            }

            return TableConfiguration.AllMetadataColumns.Contains(
                self,
                ColumnConfigurationEqualityComparer.Default);
        }

        /// <summary>
        ///     Determines whether the given value is valid as a <see cref="ColumnRole"/>.
        /// </summary>
        /// <param name="self">
        ///     The value to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the given <see cref="ColumnRole"/> is valid;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsValidColumnRole(this ColumnRole self)
        {
            return Enum.IsDefined(typeof(ColumnRole), self) &&
                self != ColumnRole.CountColumnMetadata;
        }
    }
}
