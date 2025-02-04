// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Performance.SDK.Auth;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Options;

namespace Microsoft.Performance.Toolkit.Engine
{
    internal sealed class EngineApplicationEnvironment
        : ApplicationEnvironment
    {
        private readonly ReadOnlyDictionary<Type, object> authProviders;

        public EngineApplicationEnvironment(
            string applicationName,
            string runtimeName,
            ITableDataSynchronization tableDataSynchronizer,
            ISourceDataCookerFactoryRetrieval sourceDataCookerFactory,
            ISourceSessionFactory sourceSessionFactory,
            IMessageBox messageBox,
            ReadOnlyDictionary<Type, object> authProviders,
            PluginOptionsRegistry optionsRegistry)
            : base(
                applicationName,
                runtimeName,
                tableDataSynchronizer,
                sourceDataCookerFactory,
                sourceSessionFactory,
                messageBox,
                optionsRegistry)
        {
            this.authProviders = authProviders;
        }

        /// <inheritdoc />
        public override bool TryGetAuthProvider<TAuth, TResult>(out IAuthProvider<TAuth, TResult> provider)
        {
            if (this.authProviders.TryGetValue(typeof(TAuth), out var authProvider))
            {
                var authProviderTyped = authProvider as IAuthProvider<TAuth, TResult>;

                if (authProviderTyped == null)
                {
                    Debug.Fail($"Expected registered {nameof(authProvider)} for type {typeof(TAuth)} to be of type {nameof(IAuthProvider<TAuth, TResult>)}");

                    provider = null;
                    return false;
                }

                provider = authProviderTyped;
                return true;
            }

            return base.TryGetAuthProvider(out provider);
        }
    }
}