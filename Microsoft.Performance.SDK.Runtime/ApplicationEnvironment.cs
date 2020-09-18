// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using System;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <inheritdoc cref="IApplicationEnvironment"/>
    public class ApplicationEnvironment
        : IApplicationEnvironment
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
        /// <param name="serializer">
        ///     Used to serialize/deserialize data (e.g. table configurations).
        /// </param>
        /// <param name="dataCookers">
        ///     A repository of source data cookers.
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
            ISerializer serializer,
            ISourceDataCookerRepository dataCookers,
            ISourceSessionFactory sourceSessionFactory,
            IMessageBox messageBox)
        {
            // application and runtime names may be null

            Guard.NotNull(tableDataSynchronizer, nameof(tableDataSynchronizer));
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(dataCookers, nameof(dataCookers));
            Guard.NotNull(sourceSessionFactory, nameof(sourceSessionFactory));
            Guard.NotNull(messageBox, nameof(messageBox));

            this.ApplicationName = applicationName ?? string.Empty;
            this.RuntimeName = runtimeName ?? string.Empty;

            this.messageBox = messageBox;

            // _CDS_
            // todo:when tests are ready, consider checking that columnController is not null

            this.Serializer = serializer;
            this.TableDataSynchronizer = tableDataSynchronizer;
            this.SourceDataCookerFactoryRetrieval = dataCookers;
            this.SourceSessionFactory = sourceSessionFactory;
        }

        /// <summary>
        ///     Gets the name of the application.
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        ///     Gets the name of the runtime on which the application is built, if any.
        /// </summary>
        public string RuntimeName { get; }

        /// <summary>
        ///     Gets a value that indicates whether the process is running in a graphical user environment.
        /// </summary>
        public bool GraphicalUserEnvironment { get; set; }

        /// <summary>
        ///     Gets an object to serialize/deserialize data (e.g. table configurations).
        /// </summary>
        public ISerializer Serializer { get; }

        /// <summary>
        ///     Gets an object used to synchronize table data changes with the user interface.
        /// </summary>
        public ITableDataSynchronization TableDataSynchronizer { get; }

        /// <summary>
        ///     Gets a value that indicates if verbose output is enabled.
        /// </summary>
        public bool VerboseOutput { get; set; }

        /// <summary>
        ///     Gets an object that is used to retrieve an <see cref="ISourceDataCookerFactory"/>.
        /// </summary>
        public ISourceDataCookerFactoryRetrieval SourceDataCookerFactoryRetrieval { get; }

        /// <summary>
        ///     Gets an object that is used to create an <see cref="ISourceProcessingSession{T, TKey, TContext}"/>
        ///     for a data processor.
        /// </summary>
        public ISourceSessionFactory SourceSessionFactory { get; }

        /// <summary>
        ///     Displays a message using an <see cref="IMessageBox"/>.
        /// </summary>
        /// <param name="messageType">
        ///     The type of message to display.
        /// </param>
        /// <param name="formatProvider">
        ///     Provides a mechanism for retrieving an object to control message formatting.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An object array that contains zero or more objects to format.
        /// </param>
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

        /// <summary>
        ///     Displays a message using an <see cref="IMessageBox"/>.
        /// </summary>
        /// <param name="messageType">
        ///     The type of message to display.
        /// </param>
        /// <param name="formatProvider">
        ///     Provides a mechanism for retrieving an object to control message formatting.
        /// </param>
        /// <param name="buttons">
        ///     A value that specifies which button or buttons to display.
        /// </param>
        /// <param name="caption">
        ///     A <c>string</c> that specifies the title bar caption to display.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     An object array that contains zero or more objects to format.
        /// </param>
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
