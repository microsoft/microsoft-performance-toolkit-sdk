// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.DTO;
using System;
using System.Diagnostics;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables
{
    internal class TableExtensionReference
        : DataExtensionReference<TableExtensionReference>,
          ITableExtensionReference
    {
        private static ISerializer tableConfigSerializer = new TableConfigurationsSerializer();

        internal static bool TryCreateReference(
            Type candidateType,
            out ITableExtensionReference reference)
        {
            return TryCreateReference(candidateType, false, out reference);
        }

        internal static bool TryCreateReference(
            Type candidateType,
            bool allowInternalTables,
            out ITableExtensionReference reference)
        {
            Guard.NotNull(candidateType, nameof(candidateType));

            reference = null;

            if (TableDescriptorFactory.TryCreate(
                    candidateType,
                    tableConfigSerializer,
                    out var isInternalTable,
                    out var tableDescriptor,
                    out var tableBuildAction,
                    out var tableIsDataAvailableFunc))
            {
                if (isInternalTable)
                {
                    if (!allowInternalTables)
                    {
                        return false;
                    }

                    Debug.Assert(tableBuildAction != null);
                }

                var tableReference = new TableExtensionReference(
                    candidateType,
                    tableDescriptor,
                    tableBuildAction,
                    tableIsDataAvailableFunc);

                tableReference.IsInternalTable = isInternalTable;

                reference = tableReference;
            }

            return reference != null;
        }

        private TableExtensionReference(
            Type type,
            TableDescriptor tableDescriptor,
            Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction,
            Func<IDataExtensionRetrieval, bool> isDataAvailableFunc) 
            : base(type)
        {
            Guard.NotNull(tableDescriptor, nameof(tableDescriptor));

            this.TableDescriptor = tableDescriptor;
            this.BuildTableAction = buildTableAction;
            this.IsDataAvailableFunc = isDataAvailableFunc;

            foreach (var dataCookerPath in tableDescriptor.RequiredDataCookers)
            {
                AddRequiredDataCooker(dataCookerPath);
            }

            foreach (var dataProcessorId in tableDescriptor.RequiredDataProcessors)
            {
                AddRequiredDataProcessor(dataProcessorId);
            }
        }

        public TableExtensionReference(TableExtensionReference other) 
            : base(other)
        {
            this.TableDescriptor = other.TableDescriptor;
            this.BuildTableAction = other.BuildTableAction;
            this.IsDataAvailableFunc = other.IsDataAvailableFunc;
        }

        public TableDescriptor TableDescriptor { get; }

        public Action<ITableBuilder, IDataExtensionRetrieval> BuildTableAction { get; }

        public Func<IDataExtensionRetrieval, bool> IsDataAvailableFunc { get; }

        internal bool IsInternalTable { get; private set; }

        public override TableExtensionReference CloneT()
        {
            return new TableExtensionReference(this);
        }

        public override bool Equals(TableExtensionReference other)
        {
            return base.Equals(other) &&
                   (this.TableDescriptor.Guid == other.TableDescriptor.Guid);
        }
    }
}
