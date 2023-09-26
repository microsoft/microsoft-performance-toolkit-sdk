// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.DTO;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables
{
    /// <summary>
    ///     References a table extension.
    /// </summary>
    internal class TableExtensionReference
        : DataExtensionReference,
          ITableExtensionReference,
          IEquatable<TableExtensionReference>
    {
        private static ITableConfigurationsSerializer tableConfigSerializer = new TableConfigurationsSerializer();

        private TableDescriptor tableDescriptor;
        private Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction;
        private Func<IDataExtensionRetrieval, bool> isDataAvailableFunc;

        private bool isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableExtensionReference"/>
        ///     class as a copy of the given instance.
        /// </summary>
        /// <param name="other">
        ///     The instance from which to initialize this instance.
        /// </param>
        /// <exception cref="System.ObjectDisposedException">
        ///     <paramref name="other"/> is disposed.
        /// </exception>
        public TableExtensionReference(TableExtensionReference other)
            : base(other)
        {
            this.tableDescriptor = other.tableDescriptor;
            this.buildTableAction = other.buildTableAction;
            this.isDataAvailableFunc = other.isDataAvailableFunc;
        }

        private TableExtensionReference(
            string tableName,
            TableDescriptor tableDescriptor,
            Action<ITableBuilder, IDataExtensionRetrieval> buildTableAction,
            Func<IDataExtensionRetrieval, bool> isDataAvailableFunc)
            : base(tableName)
        {
            Debug.Assert(tableDescriptor != null);
            Debug.Assert(buildTableAction != null);

            this.tableDescriptor = tableDescriptor;
            this.buildTableAction = buildTableAction;
            this.isDataAvailableFunc = isDataAvailableFunc;

            foreach (var dataCookerPath in tableDescriptor.RequiredDataCookers)
            {
                this.AddRequiredDataCooker(dataCookerPath);
            }

            // TODO: __SDK_DP__
            //foreach (var dataProcessorId in tableDescriptor.RequiredDataProcessors)
            //{
            //    this.AddRequiredDataProcessor(dataProcessorId);
            //}

            this.isDisposed = false;
        }

        /// <summary>
        ///     Gets the descriptor of the referenced table.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public TableDescriptor TableDescriptor
        {
            get
            {
                this.ThrowIfDisposed();
                return this.tableDescriptor;
            }
        }

        /// <summary>
        ///     Gets the action to use to build the referenced table.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public Action<ITableBuilder, IDataExtensionRetrieval> BuildTableAction
        {
            get
            {
                this.ThrowIfDisposed();
                return this.buildTableAction;
            }
        }

        /// <summary>
        ///     Gets the function that determines whether the referenced
        ///     table has data.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public Func<IDataExtensionRetrieval, bool> IsDataAvailableFunc
        {
            get
            {
                this.ThrowIfDisposed();
                return this.isDataAvailableFunc;
            }
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="ITableExtensionReference"/> based on the
        ///     <paramref name="candidateType"/>.
        ///     <para/>
        ///     A <see cref="Type"/> must satisfy the following criteria in order to 
        ///     be eligible as a reference:
        ///     <list type="bullet">
        ///         <item>
        ///             must expose a valid <see cref="TableDescriptor"/>.
        ///             See <see cref="TableDescriptorFactory.TryCreate(Type, ITableConfigurationsSerializer, out TableDescriptor)"/>
        ///             for details on how a table is to expose a descriptor.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="candidateType">
        ///     Candidate <see cref="Type"/> for the <see cref="ITableExtensionReference"/>
        /// </param>
        /// <param name="logger">
        ///     Logs messages during reference creation.
        /// </param>
        /// <param name="reference">
        ///     Out <see cref="ITableExtensionReference"/>
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <paramref name="candidateType"/> is valid and can create a instance of <see cref="ISourceDataCookerReference"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="candidateType"/> is <c>null</c>.
        /// </exception>
        internal static bool TryCreateReference(
            Type candidateType,
            ILogger logger,
            out ITableExtensionReference reference)
        {
            Debug.Assert(candidateType != null, $"{nameof(candidateType)} cannot be null.");
            Debug.Assert(logger != null, $"{nameof(logger)} cannot be null.");

            reference = null;

            if (TableDescriptorFactory.TryCreate(
                    candidateType,
                    tableConfigSerializer,
                    logger,
                    out var tableDescriptor,
                    out var tableBuildAction,
                    out var tableIsDataAvailableFunc))
            {
                Debug.Assert(tableDescriptor != null);

                // Extension tables must have a build action

                if (tableBuildAction != null)
                {
                    try
                    {
                        reference = CreateReference(
                            tableDescriptor,
                            tableBuildAction,
                            tableIsDataAvailableFunc);
                    }
                    catch (Exception)
                    {
                        Debug.Assert(false, "What threw in this method?");
                    }
                }
            }

            return reference != null;
        }

        /// <summary>
        ///     Tries to create an instance of <see cref="ITableExtensionReference"/> based on the
        ///     <paramref name="candidateType"/>.
        ///     <para/>
        ///     A <see cref="Type"/> must satisfy the following criteria in order to 
        ///     be eligible as a reference:
        ///     <list type="bullet">
        ///         <item>
        ///             must expose a valid <see cref="TableDescriptor"/>.
        ///             See <see cref="TableDescriptorFactory.TryCreate(Type, ITableConfigurationsSerializer, out TableDescriptor)"/>
        ///             for details on how a table is to expose a descriptor.
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The table's <see cref="Processing.TableDescriptor"/>.
        /// </param>
        /// <param name="tableBuildAction">
        ///     The action used to build the table. May be <c>null</c>.
        /// </param>
        /// <param name="tableIsDataAvailableFunc">
        ///     A function used to determine if the table has data. May be <c>null</c>.
        /// </param>
        /// <returns>
        ///     The <see cref="ITableExtensionReference"/> for the given <see cref="Processing.TableDescriptor"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="tableDescriptor"/> is <c>null</c>.
        ///     -- or --
        ///     <paramref name="tableBuildAction"/> is <c>null</c>.
        /// </exception>
        internal static ITableExtensionReference CreateReference(
            TableDescriptor tableDescriptor,
            Action<ITableBuilder, IDataExtensionRetrieval> tableBuildAction,
            Func<IDataExtensionRetrieval, bool> tableIsDataAvailableFunc)
        {
            Guard.NotNull(tableDescriptor, nameof(tableDescriptor));
            Guard.NotNull(tableBuildAction, nameof(tableBuildAction));

            var tableName = tableDescriptor.Type?.FullName
                ?? tableDescriptor.Name + "{" + tableDescriptor.Guid.ToString() + "}";

            return new TableExtensionReference(
                tableName,
                tableDescriptor,
                tableBuildAction,
                tableIsDataAvailableFunc);
        }

        /// <inheritdoc />
        public bool Equals(TableExtensionReference other)
        {
            return base.Equals(other) &&
                   (this.TableDescriptor.Guid == other.TableDescriptor.Guid);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.tableDescriptor = null;
                this.buildTableAction = null;
                this.isDataAvailableFunc = null;
            }

            this.isDisposed = true;
            base.Dispose(disposing);
        }
    }
}
