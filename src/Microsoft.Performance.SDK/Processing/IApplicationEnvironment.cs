// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using System;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Exposes information about the application environment in
    ///     which the custom data source is being executed.
    /// </summary>
    public interface IApplicationEnvironment
    {
        /// <summary>
        /// The name of the application, if specified. This value may be <c>null</c>.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// The name of the runtime in which this application runs. This value may be <c>null</c>.
        /// </summary>
        string RuntimeName { get; }

        /// <summary>
        ///     Gets a value indicating whether a GUI is present in the
        ///     current process.
        /// </summary>
        /// <remarks>
        ///     This flag is useful to determine whether the code is
        ///     running in a UI or CLI tool.
        /// </remarks>
        bool GraphicalUserEnvironment { get; }

        /// <summary>
        ///     Gets the serializer to use for deserializing table configurations.
        /// </summary>
        ISerializer Serializer { get; }

        /// <summary>
        ///     Provides the interface to be used to notify that data in
        ///     a table has changed.
        /// </summary>
        ITableDataSynchronization TableDataSynchronizer { get; }

        /// <summary>
        ///     Gets a value indicating whether Verbose output has
        ///     been enabled 
        /// </summary>
        bool VerboseOutput { get; }

        /// <summary>
        ///     Used to get a factory for a given source data cooker.
        /// </summary>
        ISourceDataCookerFactoryRetrieval SourceDataCookerFactoryRetrieval { get; }

        /// <summary>
        ///     A factory to create a SourceSession.
        /// </summary>
        ISourceSessionFactory SourceSessionFactory { get; }

        /// <summary>
        ///     Displays the given message of the given type to the user,
        ///     using the formatting information specified by the <paramref name="formatProvider"/>.
        ///     The message is displayed in a message box.
        /// </summary>
        /// <param name="messageType">
        ///     The type of message being displayed.
        /// </param>
        /// <param name="formatProvider">
        ///     An object that supplies culture-specific formatting information.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to format using <paramref name="format"/>.
        /// </param>
        void DisplayMessage(
            MessageType messageType,
            IFormatProvider formatProvider,
            string format,
            params object[] args);

        /// <summary>
        ///     Displays the given message of the given type to the user,
        ///     using the formatting information specified by the <paramref name="formatProvider"/>.
        ///     The message is displayed in a message box with buttons.
        /// </summary>
        /// <param name="messageType">
        ///     The type of message being displayed.
        /// </param>
        /// <param name="formatProvider">
        ///     An object that supplies culture-specific formatting information.
        /// </param>
        /// <param name="buttons">
        ///     The buttons on the message box.
        /// </param>
        /// <param name="caption">
        ///     A simple description about what is being asked.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="args">
        ///     The objects to format using <paramref name="format"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the user selects 'yes'; <c>false</c>
        ///     otherwise.
        /// </returns>
        ButtonResult MessageBox(
            MessageType messageType,
            IFormatProvider formatProvider,
            Buttons buttons,
            string caption,
            string format,
            params object[] args);
    }
}
