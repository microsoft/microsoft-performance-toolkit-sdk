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

        private readonly TableDescriptor tableDescriptor;
        private readonly Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction;
        private readonly Func<IDataExtensionRetrieval, bool> isDataAvailableFunc;
        private readonly bool isInternalTable;

        private bool isDisposed;

        public TableExtensionReference(TableExtensionReference other)
            : base(other)
        {
            this.tableDescriptor = other.tableDescriptor;
            this.buildTableAction = other.buildTableAction;
            this.isDataAvailableFunc = other.isDataAvailableFunc;
            this.isInternalTable = other.isInternalTable;
        }

        private TableExtensionReference(
            Type type,
            TableDescriptor tableDescriptor,
            Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction,
            Func<IDataExtensionRetrieval, bool> isDataAvailableFunc,
            bool isInternalTable)
            : base(type)
        {
            Guard.NotNull(tableDescriptor, nameof(tableDescriptor));

            this.tableDescriptor = tableDescriptor;
            this.buildTableAction = buildTableAction;
            this.isDataAvailableFunc = isDataAvailableFunc;
            this.isInternalTable = isInternalTable;

            foreach (var dataCookerPath in tableDescriptor.RequiredDataCookers)
            {
                this.AddRequiredDataCooker(dataCookerPath);
            }

            foreach (var dataProcessorId in tableDescriptor.RequiredDataProcessors)
            {
                this.AddRequiredDataProcessor(dataProcessorId);
            }

            this.isDisposed = false;
        }

        ~TableExtensionReference()
        {
        }

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
                    tableIsDataAvailableFunc,
                    isInternalTable);

                reference = tableReference;
            }

            return reference != null;
        }

        public TableDescriptor TableDescriptor
        {
            get
            {
                this.ThrowIfDisposed();
                return this.tableDescriptor;
            }
        }

        public Action<ITableBuilder, IDataExtensionRetrieval> BuildTableAction
        {
            get
            {
                this.ThrowIfDisposed();
                return this.buildTableAction;
            }
        }

        public Func<IDataExtensionRetrieval, bool> IsDataAvailableFunc
        {
            get
            {
                this.ThrowIfDisposed();
                return this.isDataAvailableFunc;
            }
        }

        internal bool IsInternalTable
        {
            get
            {
                this.ThrowIfDisposed();
                return this.isInternalTable;
            }
        }

        public override TableExtensionReference CloneT()
        {
            this.ThrowIfDisposed();
            return new TableExtensionReference(this);
        }

        public override bool Equals(TableExtensionReference other)
        {
            return base.Equals(other) &&
                   (this.TableDescriptor.Guid == other.TableDescriptor.Guid);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.isDisposed = true;
        }
    }
}
