// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Engine.Tests.Driver
{
    internal abstract class TestCommand
    {
        private readonly bool debugger;

        protected TestCommand(
            string pluginDirectory,
            bool debugger,
            bool verbose)
        {
            this.debugger = debugger;

            this.PluginDirectory = pluginDirectory;

            var consoleLog = ConsoleLogger.Create(this.GetType());
            this.Log = new ConfigurableLogger(consoleLog)
            {
                IsVerboseEnabled = verbose,
            };
        }

        /// <summary>
        ///     Gets the directory from which to load plugins (if applicable.)
        /// </summary>
        private protected string PluginDirectory { get; }

        /// <summary>
        ///     Gets the logger for logging messages from the command.
        /// </summary>
        private protected ILogger Log { get; }

        private protected static ICommandHandler CreateHandler(Type type, string methodName)
        {
            var method = type.GetMethod(
                    methodName,
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);
            if (method is null)
            {
                throw new MissingMethodException(type.FullName, methodName);
            }

            return CommandHandler.Create(method);
        }

        /// <summary>
        ///     Executes this test command.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A token that may be queried to determine if the caller is requesting that the currently
        ///     running operation be cancelled.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the result of the asynchronous operation. A return value
        ///     of zero (0) indicates success; all other values indicate failure.
        /// </returns>
        internal async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            this.Log.Info("Executing test command `{0}`", this.GetType());

            if (this.debugger)
            {
                this.Log.Info("Waiting for a debugger to attach...");

                try
                {
                    while (!NativeMethods.IsDebuggerPresent())
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    this.Log.Info("Test command `{0}` was cancelled.", this.GetType());
                    return 1;
                }

                this.Log.Info("Debugger attached; breaking...");
                NativeMethods.DebugBreak();
            }

            int result;
            try
            {
                result = await this.ExecuteCoreAsync(cancellationToken);
                this.Log.Info("Test command `{0}` completed with exit code {1}", this.GetType(), result);
            }
            catch (OperationCanceledException)
            {
                this.Log.Info("Test command `{0}` was cancelled.", this.GetType());
                result = 1;
            }
            catch (Exception e)
            {
                this.Log.Error(e, "Test command `{0}` encountered an exception.", this.GetType());
                result = e.HResult;
            }
            finally
            {
                try
                {
                    await this.OnExecutedAsync();
                }
                catch (Exception e)
                {
                    this.Log.Fatal(e, "An unexpected exception occurred in the test cleanup for `{0}`", this.GetType());
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        ///     When overridden in a derived class, executes the main test logic.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A token that may be queried to determine if the caller is requesting that the currently
        ///     running operation be cancelled.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the result of the asynchronous operation. A return value
        ///     of zero (0) indicates success; all other values indicate failure.
        /// </returns>
        private protected abstract Task<int> ExecuteCoreAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     When overridden in a derived class, performs any logic that should be executed after the
        ///     main test command completes. This method is *ALWAYS* called after ExecuteAsync returns.
        /// </summary>
        /// <returns>
        ///     A <see cref="Task"/> representing the result of the asynchronous operation.
        /// </returns>
        private protected virtual Task OnExecutedAsync()
        {
            return Task.CompletedTask;
        }

        private sealed class ConfigurableLogger
            : ILogger
        {
            private readonly ILogger log;

            internal ConfigurableLogger(ILogger log)
            {
                Debug.Assert(log != null);

                this.log = log;
            }

            internal bool IsVerboseEnabled { get; set; }

            public void Error(string fmt, params object[] args)
            {
                this.log.Error(fmt, args);
            }

            public void Error(Exception e, string fmt, params object[] args)
            {
                this.log.Error(e, fmt, args);
            }

            public void Fatal(string fmt, params object[] args)
            {
                this.log.Fatal(fmt, args);
            }

            public void Fatal(Exception e, string fmt, params object[] args)
            {
                this.log.Fatal(e, fmt, args);
            }

            public void Info(string fmt, params object[] args)
            {
                this.log.Info(fmt, args);
            }

            public void Info(Exception e, string fmt, params object[] args)
            {
                this.log.Info(e, fmt, args);
            }

            public void Verbose(string fmt, params object[] args)
            {
                if (this.IsVerboseEnabled)
                {
                    this.log.Verbose(fmt, args);
                }
            }

            public void Verbose(Exception e, string fmt, params object[] args)
            {
                if (this.IsVerboseEnabled)
                {
                    this.log.Verbose(e, fmt, args);
                }
            }

            public void Warn(string fmt, params object[] args)
            {
                this.log.Warn(fmt, args);
            }

            public void Warn(Exception e, string fmt, params object[] args)
            {
                this.log.Warn(e, fmt, args);
            }
        }
    }
}
