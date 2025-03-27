// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Auth;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Options.Values;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.DTO;
using Microsoft.Performance.SDK.Runtime.Options;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <inheritdoc cref="IApplicationEnvironment"/>
    public class ApplicationEnvironment
        : IApplicationEnvironmentV3
    {
        private readonly IMessageBox messageBox;
        private readonly PluginOptionsRegistry optionsRegistry;

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
        /// <param name="optionsRegistry">
        ///     The registry of plugin options to use for the application.
        /// </param>
        public ApplicationEnvironment(
            string applicationName,
            string runtimeName,
            ITableDataSynchronization tableDataSynchronizer,
            ISourceDataCookerFactoryRetrieval sourceDataCookerFactory,
            ISourceSessionFactory sourceSessionFactory,
            IMessageBox messageBox,
            PluginOptionsRegistry optionsRegistry)
        {
            // application and runtime names may be null
            // tableDataSynchronizer may be null

            Guard.NotNull(sourceDataCookerFactory, nameof(sourceDataCookerFactory));
            Guard.NotNull(sourceSessionFactory, nameof(sourceSessionFactory));
            Guard.NotNull(messageBox, nameof(messageBox));

            this.ApplicationName = applicationName ?? string.Empty;
            this.RuntimeName = runtimeName ?? string.Empty;

            this.messageBox = messageBox;

            // _CDS_
            // todo:when tests are ready, consider checking that columnController is not null

            this.Serializer = new TableConfigurationsSerializer();
            this.TableDataSynchronizer = tableDataSynchronizer ?? new NullTableSynchronizer();
            this.SourceDataCookerFactoryRetrieval = sourceDataCookerFactory;
            this.SourceSessionFactory = sourceSessionFactory;
            this.optionsRegistry = optionsRegistry;
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

        public bool TryGetPluginOption<T>(Guid optionGuid, out T option) where T : PluginOptionValue
        {
            var visitor = new PluginOptionValueFinder<T>(optionGuid);
            new PluginOptionVisitorExecutor(visitor).Visit(this.optionsRegistry.Options);

            option = visitor.FoundValue;
            return option != null;
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

        private sealed class NullTableSynchronizer
            : ITableDataSynchronization
        {
            public void SubmitColumnChangeRequest(
                IEnumerable<Guid> columns,
                Action onReadyForChange,
                Action onChangeComplete,
                bool requestInitialFilterReevaluation = false)
            {
                onReadyForChange?.Invoke();
                onChangeComplete?.Invoke();
            }

            public void SubmitColumnChangeRequest(
                Func<IProjectionDescription, bool> predicate,
                Action onReadyForChange,
                Action onChangeComplete,
                bool requestInitialFilterReevaluation = false)
            {
                onReadyForChange?.Invoke();
                onChangeComplete?.Invoke();
            }
        }

        private sealed class PluginOptionValueFinder<T>
            : IPluginOptionVisitor
            where T : PluginOptionValue
        {
            private readonly Guid guidToFind;

            public PluginOptionValueFinder(Guid guidToFind)
            {
                this.guidToFind = guidToFind;
            }

            public T FoundValue { get; private set; }

            public void Visit(BooleanOption option)
            {
                Visit(option.Definition.Guid, option.Value);
            }

            public void Visit(FieldOption option)
            {
                Visit(option.Definition.Guid, option.Value);
            }

            public void Visit(FieldArrayOption option)
            {
                Visit(option.Definition.Guid, option.Value);
            }

            private void Visit(Guid guid, PluginOptionValue value)
            {
                if (guid != this.guidToFind)
                {
                    return;
                }

                if (value is T optionValue)
                {
                    this.FoundValue = optionValue;
                }
            }
        }
    }
}
