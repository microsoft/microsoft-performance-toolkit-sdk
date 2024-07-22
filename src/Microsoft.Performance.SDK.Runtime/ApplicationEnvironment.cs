// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Auth;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.DTO;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <inheritdoc cref="IApplicationEnvironment"/>
    public class ApplicationEnvironment
        : IApplicationEnvironmentV2
    {
        private readonly IMessageBox messageBox;

        /// <summary>
        ///     Creates an instance of the <see cref="ApplicationEnvironment"/> class.
        /// </summary>
        /// <param name="applicationName">
        ///     Name of the application.
        /// </param>
        /// <param name="runtimeName">
        ///     Name of the runtime on which the application is built.
        /// </param>
        /// <param name="tableDataSynchronizer">
        ///     Used to synchronize table data changes with the user interface.
        /// </param>
        /// <param name="sourceDataCookerFactory">
        ///     Provides a way to create instances of data cookers of type <see cref="DataCookerType.SourceDataCooker"/>.
        /// </param>
        /// <param name="sourceSessionFactory">
        ///     Provides a way to create an <see cref="ISourceProcessingSession{T, TKey, TContext}"/> for a data processor.
        /// </param>
        /// <param name="messageBox">
        ///     Provides a way to message the user.
        /// </param>
        public ApplicationEnvironment(
            string applicationName,
            string runtimeName,
            ITableDataSynchronization tableDataSynchronizer,
            ISourceDataCookerFactoryRetrieval sourceDataCookerFactory,
            ISourceSessionFactory sourceSessionFactory,
            IMessageBox messageBox)
        {
            // application and runtime names may be null

            Guard.NotNull(tableDataSynchronizer, nameof(tableDataSynchronizer));
            Guard.NotNull(sourceDataCookerFactory, nameof(sourceDataCookerFactory));
            Guard.NotNull(sourceSessionFactory, nameof(sourceSessionFactory));
            Guard.NotNull(messageBox, nameof(messageBox));

            this.ApplicationName = applicationName ?? string.Empty;
            this.RuntimeName = runtimeName ?? string.Empty;

            this.messageBox = messageBox;

            // _CDS_
            // todo:when tests are ready, consider checking that columnController is not null

            this.Serializer = new TableConfigurationsSerializer();
            this.TableDataSynchronizer = tableDataSynchronizer;
            this.SourceDataCookerFactoryRetrieval = sourceDataCookerFactory;
            this.SourceSessionFactory = sourceSessionFactory;
        }

        /// <inheritdoc />
        public string ApplicationName { get; }

        /// <inheritdoc />
        public string RuntimeName { get; }

        /// <inheritdoc />
        public bool IsInteractive { get; set; }

        /// <inheritdoc />
        public ITableConfigurationsSerializer Serializer { get; }

        /// <inheritdoc />
        public ITableDataSynchronization TableDataSynchronizer { get; }

        /// <inheritdoc />
        public bool VerboseOutput { get; set; }

        /// <inheritdoc />
        public ISourceDataCookerFactoryRetrieval SourceDataCookerFactoryRetrieval { get; }

        /// <inheritdoc />
        public ISourceSessionFactory SourceSessionFactory { get; }

        /// <inheritdoc />
        public void DisplayMessage(
            MessageType messageType,
            IFormatProvider formatProvider,
            string format,
            params object[] args)
        {
            this.messageBox.Show(
                MapToImage(messageType),
                formatProvider,
                format,
                args);
        }

        /// <inheritdoc />
        public ButtonResult MessageBox(
             MessageType messageType,
             IFormatProvider formatProvider,
             Buttons buttons,
             string caption,
             string format,
             params object[] args)
        {
            return this.messageBox.Show(
                MapToImage(messageType),
                formatProvider,
                buttons,
                caption,
                format,
                args);
        }

        /// <inheritdoc />
        public virtual bool TryGetAuthProvider<TAuth, TResult>(out IAuthProvider<TAuth, TResult> provider)
            where TAuth : IAuthMethod<TResult>
        {
            provider = null;
            return false;
        }

        private static MessageBoxIcon MapToImage(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Error:
                    return MessageBoxIcon.Error;

                case MessageType.Warning:
                    return MessageBoxIcon.Warning;

                case MessageType.Information:
                // this is the default. Fall through

                default:
                    return MessageBoxIcon.Information;
            }
        }
    }
}
